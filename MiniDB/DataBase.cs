using MiniDB.Interfaces;
using MiniDB.Transactions;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace MiniDB
{
    public class DataBase : ObservableCollection<IDBObject>, IDisposable
    {
        #region fields

        /// <summary>
        /// lock object - this is used to lock the current database from attempting to edit the data at the same time.
        /// </summary>
        protected static readonly object Locker = new object();

        private readonly IStorageStrategy storageStrategy;

        private readonly IUndoRedoManager undoRedoManager;

        /// <summary>
        /// http://www.albahari.com/threading/part2.aspx#_Mutex
        /// Create mutex in constructor - name it so that only one instance of db class can be accessing file
        ///  - this allows for multiple instances of a DB, but only one of a given type accessing a given file
        /// </summary>
        private Mutex mut = null;

        #endregion

        #region Constructors

        public DataBase(string filename, float version, float minimumCompatibleVersion, IStorageStrategy storageStrategy, IUndoRedoManager undoRedoManager = null) : base()
        {
            this.Filename = filename;
            this.Filename = Path.GetFullPath(this.Filename); // use the full system path - especially for mutex to know if it needs to lock that file or not

            this.storageStrategy = storageStrategy;
            this.undoRedoManager = undoRedoManager;

            this.DBVersion = version;
            this.MinimumCompatibleVersion = minimumCompatibleVersion;

            lock (Locker)
            {
                this.getMutex();

                try
                {
                    TransactionsFilename = string.Format(@"{0}\transactions_{1}.data", Path.GetDirectoryName(this.Filename), Path.GetFileName(this.Filename));
                    this.Transactions_DB = this.storageStrategy._getTransactionsCollection(TransactionsFilename);
                    this.Transactions_DB.CollectionChanged += this.DataBase_TransactionsChanged;

                    this.LoadFile(filename, true);

                    this.CollectionChanged += this.DataBase_CollectionChanged;
                }
                catch
                {
                    this.Dispose();
                    throw;
                }
            }
        }

        /// <summary>
        /// Setup a basic DB class that does not have the ability to load or write to disk.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="version"></param>
        /// <param name="minimumCompatibleVersion"></param>
        public DataBase(string filename, float version, float minimumCompatibleVersion) : base()
        {
            this.Filename = filename;
            this.Filename = Path.GetFullPath(this.Filename); // use the full system path - especially for mutex to know if it needs to lock that file or not

            this.DBVersion = version;
            this.MinimumCompatibleVersion = minimumCompatibleVersion;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataBase{T}" /> class.
        /// Default constructor (allow Newtonsoft to create object without parameters
        /// </summary>
        [JsonConstructor]
        internal DataBase() : base()
        {
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // clear mutex
                    if (this.mut != null && this.mut.WaitOne(TimeSpan.FromSeconds(5), false))
                    {
                        this.mut.ReleaseMutex();
                        this.mut.Close();
                        this.mut = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                this.disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DataBase() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        #region Properties

        public string Filename { get; }
        private string TransactionsFilename { get; }
        public float DBVersion { get; internal set; }
        public float MinimumCompatibleVersion { get; }

        private ObservableCollection<IDBTransaction> Transactions_DB { get; }

        /// <summary>
        /// Gets a value indicating whether the collection currently can undo a recent transaction
        /// </summary>
        public bool CanUndo
        {
            get
            {
                return this.undoRedoManager.CheckCanUndo(this.Transactions_DB);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the collection currently can redo an undone change
        /// </summary>
        public bool CanRedo
        {
            get
            {
                return this.undoRedoManager.CheckCanRedo(this.Transactions_DB);
            }
        }

        #endregion

        #region events/delegates

        /// <summary>
        /// Delegate to specify what handlers of ItemChanged shoud look like
        /// </summary>
        /// <param name="sender">Istance of T that chaged</param>
        /// <param name="id">Database ID for T object</param>
        public delegate void TChangedEventHandler(object sender, ID id);

        /// <summary>
        /// Public event raised when an item is changed
        /// </summary>
        public event TChangedEventHandler ItemChanged;

        /// <summary>
        /// Public event for when a DB property has changed
        /// </summary>
        public event PropertyChangedEventHandler PublicPropertyChanged;

        #endregion

        #region methods

        #region API

        /// <summary>
        /// Remove all elements from DB
        /// </summary>
        public new void Clear()
        {
            ///
            /// Ordinarily, Clear wipes all elements then calls our collectionChanged with a NotifyCollectionChangedAction.Reset,
            /// but the items are removed by then (and the OldItems in the event is null), so override the clear to remove each item so we can log it.

            var tempItems = new Collection<IDBObject>();
            foreach(var item in this)
            {
                tempItems.Add(item);
            }

            foreach (var item in tempItems)
            {
                this.Remove(item);
            }
        }

        public void Undo()
        {
            if(!this.undoRedoManager.CheckCanUndo(this.Transactions_DB))
            {
                throw new DBCannotUndoException("Cannot undo at this time");
            }

            
            
            this.undoRedoManager.Undo(
                dataToActOn: this,
                transactions: this.Transactions_DB,
                dataChangedHandler: this.DataBase_CollectionChanged,
                transactionsChangedHandler: this.DataBase_TransactionsChanged, 
                propertyChangedHandler: this.DataBaseItem_PropertyChanged);
            this._cacheDB();

            this._alertUndoableChanged();
        }

        public void Redo()
        {
            if (!this.undoRedoManager.CheckCanRedo(this.Transactions_DB))
            {
                throw new DBCannotRedoException("Cannot redo at this time");
            }

            this.undoRedoManager.Redo(
                dataToActOn: this,
                transactions: this.Transactions_DB,
                dataChangedHandler: this.DataBase_CollectionChanged,
                transactionsChangedHandler: this.DataBase_TransactionsChanged,
                propertyChangedHandler: this.DataBaseItem_PropertyChanged);
            this._cacheDB();

            this._alertUndoableChanged();
        }

        #endregion

        #region helper methods

        private void _alertUndoableChanged()
        {
            this.PublicOnPropertyChanged(nameof(CanUndo));
            this.PublicOnPropertyChanged(nameof(CanRedo));
        }

        private void getMutex()
        {
            string mutex_file_path = this.Filename.Replace("\\", "_").Replace("/", "_");
            string mutex_name = string.Format(@"{0}:{1}", nameof(DataBase), mutex_file_path); // from https://stackoverflow.com/a/2534867
            string global_lock_mutex_name = @"Global\" + mutex_name;

            // from https://stackoverflow.com/a/3111740
            // try to get existing mutex from system
            try
            {
                this.mut = System.Threading.Mutex.OpenExisting(global_lock_mutex_name);

                // mutex already exists - not inside mutex yet, so don't need to release if thrown
                throw new DBCreationException("Another application instance is using that DB!\n\tError from: " + mutex_name);
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                this.mut = new System.Threading.Mutex(false, global_lock_mutex_name);
            }

            // acquire the lock from the mutex - this is released in dispose
            if (!this.mut.WaitOne(TimeSpan.FromSeconds(5), false))
            {
                // did not get mutex, so don't need to release if thrown
                throw new DBCreationException("Another application instance is using that DB!\n\tError from: " + mutex_name);
            }
        }

        private void LoadFile(string filename, bool registerItemsForPropertyChanged)
        {
            var data = this.storageStrategy._loadDB(filename);
            // if new enough, and not too new,
            if (data.DBVersion >= this.MinimumCompatibleVersion && data.DBVersion <= this.DBVersion)
            {
                // parse and load
                foreach (var item in data)
                {
                    this.Add(item);
                    if (registerItemsForPropertyChanged)
                    {
                        item.PropertyChangedExtended += this.DataBaseItem_PropertyChanged;
                    }
                }
            }
            else
            {
                throw new DBCreationException($"DB version {data.DBVersion} is too old. Oldest supported db version is {this.MinimumCompatibleVersion}");
            }
        }

        private void DataBaseItem_PropertyChanged(object sender, PropertyChangedExtendedEventArgs e)
        {
            lock (Locker)
            {
                var item = sender as IDBObject;
                this.ReleaseMutexOnError(() =>
                {
                    if (item == null)
                    {
                        throw new Exception($"Sender must be of type: {nameof(IDBObject)}");
                    }
                });

                this._cacheDB();

                // if not an undoable change, alert property changes and leave
                if (!e.UndoableChange)
                {
                    this.OnItemChanged(item);
                    this.PublicOnPropertyChanged(nameof(this.CanUndo));
                    this.PublicOnPropertyChanged(nameof(this.CanRedo));
                    return;
                }

                //else, store the transaction and notify
                var transaction = new ModifyTransaction(changedItemID: (sender as IDBObject).ID, fieldName: e.PropertyName, oldValue: e.OldValue, newValue: e.NewValue);

                this.Transactions_DB.Insert(0, transaction);
                this.OnItemChanged(item);
                this.PublicOnPropertyChanged(nameof(this.CanUndo));
                this.PublicOnPropertyChanged(nameof(this.CanRedo));
            }
        }

        private void DataBase_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            lock (Locker)
            {
                this._cacheDB();
            }

            // TODO: implement storing changed transaction

            // TODOne: register new items
            IList changed = null;
            

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (IDBObject item in e.NewItems)
                {
                    // register for property change
                    item.PropertyChangedExtended += this.DataBaseItem_PropertyChanged;

                    // create add transaction
                    IDBTransaction dBTransaction = new AddTransaction(item);
                    this.Transactions_DB.Insert(0, dBTransaction);
                }

                changed = e.NewItems;
            }
            else if(e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (IDBObject item in e.OldItems)
                {
                    // create remove transaction
                    // create add transaction
                    IDBTransaction dBTransaction = new DeleteTransaction(item);

                    this.Transactions_DB.Insert(0, dBTransaction);
                }

                changed = e.OldItems;
            }
            else if(e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (IDBObject item in e.OldItems)
                {
                    // create remove transaction
                    // create add transaction
                    IDBTransaction dBTransaction = new DeleteTransaction(item);

                    this.Transactions_DB.Insert(0, dBTransaction);
                }

                changed = e.OldItems;
            }
            else if(e.Action == NotifyCollectionChangedAction.Replace)
            {
                int index = 0;

                foreach (IDBObject item in e.OldItems)
                {
                    index = e.OldItems.IndexOf(item);
                    // create remove transaction
                    
                    IDBTransaction dBTransaction = new DeleteTransaction(item);
                    this.Transactions_DB.Insert(0, dBTransaction);

                    var newItem = e.NewItems[index] as IDBObject;

                    // create add transaction
                    dBTransaction = new AddTransaction(newItem);
                    this.Transactions_DB.Insert(0, dBTransaction);
                }
            }
            else
            {
                throw new NotImplementedException($"I don't know how to log transactions of type: {e.Action}");
            }
        }

        private void DataBase_TransactionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            lock (Locker)
            {
                this.storageStrategy._cacheTransactions(this.Transactions_DB, this.TransactionsFilename);
            }
        }

        /// <summary>
        /// Raise the PublicPropertyChanged event
        /// </summary>
        /// <param name="propertyName">property that changed</param>
        private void PublicOnPropertyChanged(string propertyName)
        {
            this.PublicPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raise the ItemChanged event, passing in the itemChanged's id to handlers
        /// </summary>
        /// <param name="itemChanged">the object of type T (databaseObject) that changed.</param>
        private void OnItemChanged(IDBObject itemChanged)
        {
            this.OnItemChanged(itemChanged?.ID);
        }

        /// <summary>
        /// Raise the ItemChanged event, passing in the id to handlers
        /// </summary>
        /// <param name="id">the id of the changed item</param>
        private void OnItemChanged(ID id)
        {
            this.ItemChanged?.Invoke(this, id);
        }

        /// <summary>
        /// Store the database to disk in <see cref="Filename"/>.
        /// </summary>
        protected virtual void _cacheDB()
        {
            lock (Locker)
            {
                this.storageStrategy._cacheDB(this);
            }
        }

        #region mutex helpers

        /// <summary>
        /// Invoked action and return its return value - if action throws an exception, release the mutex before re-raising
        /// </summary>
        /// <typeparam name="T2">The expected return type</typeparam>
        /// <param name="action">The method to invoke</param>
        /// <returns>The result of the action (if not error)</returns>
        private T2 ReleaseMutexOnError<T2>(Func<T2> action)
        {
            try
            {
                return action.Invoke();
            }
            catch
            {
                this.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Special case - templates can't handle a void type parameter --
        /// also <see cref="ReleaseMutexOnError{T2}(Func{T2})" />
        /// </summary>
        /// <param name="action">the action to perform</param>
        private void ReleaseMutexOnError(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch
            {
                this.Dispose();
                throw;
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
