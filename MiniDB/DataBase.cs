using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MiniDB
{
    /// <summary>
    /// Database Template: template class for persisten observable collection with undo/redo
    /// </summary>
    /// <typeparam name="T">The class type to create an observable collection of (must by a child class of DatabaseObject)</typeparam>
    public class DataBase<T> : ObservableCollection<T>, IDisposable where T : DatabaseObject
    {
        #region fields
        private readonly IStorageStrategy<IDatabaseObject> StorageStrategy;


        /// <summary>
        /// lock object - this is used to lock the current database from attempting to edit the data at the same time.
        /// </summary>
        protected static readonly object Locker = new object();

        /// <summary>
        /// The minimum compatible version that can be migrated when reloaded from disk
        /// </summary>
        protected readonly float MinimumCompatibleVersion = 1;

        /// <summary>
        /// Json Serializer Settings object used to specify how NewtonSoft should cache objects
        /// </summary>
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        /// <summary>
        /// http://www.albahari.com/threading/part2.aspx#_Mutex
        /// Create mutex in constructor - name it so that only one instance of db class can be accessing file
        ///  - this allows for multiple instances of a DB, but only one of a given type accessing a given file
        /// </summary>
        private Mutex mut = null;
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DataBase{T}" /> class.
        /// Create instance of database - if file exists, load collection from it; else, create new empty collection
        /// </summary>
        /// <param name="filename">The filename or path to store the collection in</param>
        /// <param name="databaseVersion">The current version of the database (stored only to one decimal place and max value of 25.5 - if unsure what to use, put 0.1 for now</param>
        /// <param name="minimumCompatibleVersion">The mimum compatible version - if unsure what to use, put 0 for now</param>
        /// <param name="migrate_db">Method to migrate db's that are loaded that are at least the minimum compatible version, but not the current version.</param>
        /// <param name="storageStrategy">The storage strategy to use.</param>
        public DataBase(string filename, float databaseVersion, float minimumCompatibleVersion, IStorageStrategy<IDatabaseObject> storageStrategy) : base()
        {
            this.DBVersion = databaseVersion;
            this.MinimumCompatibleVersion = minimumCompatibleVersion;
            this.StorageStrategy = storageStrategy;

            lock (Locker)
            {
                Type t = typeof(T);
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(DBTransaction<>))
                {
                    // prevent recursion - not inside mutex yet, so don't need to release if thrown
                    throw new DBCreationException("Cannot create databse of DBTransaction<T>");
                }

                filename = Path.GetFullPath(filename); // use the full system path - especially for mutex to know if it needs to lock that file or not
                string mutex_file_path = filename;
                mutex_file_path = mutex_file_path.Replace("\\", "_").Replace("/", "_");
                string mutex_name = string.Format(@"{0}<{1}>:{2}", nameof(DataBase<T>), typeof(T).Name, mutex_file_path); // from https://stackoverflow.com/a/2534867
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

                this.Filename = filename;

                string transactionFilename = string.Format(@"{0}\transactions_{1}.data", Path.GetDirectoryName(this.Filename), Path.GetFileName(this.Filename));
                this.Transactions_DB = this.StorageStrategy._getTransactionsCollection(); //this._getTransactionsDB(transactionFilename);
                this.Transactions_DB.CollectionChanged += this.DataBase_TransactionsChanged;

                this.LoadFile(filename, true);

                this.CollectionChanged += this.DataBase_CollectionChanged;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataBase{T}" /> class.
        /// Default constructor (allow Newtonsoft to create object without parameters
        /// </summary>
        [JsonConstructor]
        internal DataBase() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataBase{T}" /> class.
        /// </summary>
        /// <param name="filename">the filename (or path) to cache this collection in</param>
        /// <param name="databaseVersion">the current version of the database</param>
        /// <param name="minimumCompatibleVersion">the minimum version of a database that can be upgraded to the current version</param>
        /// <param name="base_case">used to seperate this constructor (used to create the transactions DB) from the base call</param>
        internal DataBase(string filename, float databaseVersion, float minimumCompatibleVersion, IStorageStrategy<T> storageStrategy, bool base_case) : base()
        {
            this.DBVersion = databaseVersion;
            this.Filename = filename;
            this.LoadFile(filename, false);
            this.StorageStrategy = storageStrategy;
        }
        #endregion

        #region destructors
        /// <summary>
        /// Finalizes an instance of the <see cref="DataBase{T}"/> class.
        /// double check that this gets disposed of properly
        /// </summary>
        ~DataBase()
        {
            this.Dispose();
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

        #region properties

        /// <summary>
        /// Gets a value indicating whether the collection currently can undo a recent transaction
        /// </summary>
        public bool CanUndo
        {
            get
            {
                return this.Transactions_DB.Count() > 0 && this.Transactions_DB.Count() >
                    (2 * this.Transactions_DB.Count(x => x.TransactionType == TransactionType.Undo));
            }
        }

        /// <summary>
        /// Gets a value indicating whether the collection currently can redo an undone change
        /// </summary>
        public bool CanRedo
        {
            get
            {
                // TODOne: this should be number of immediate redo's is less than number of next immediate undo's
                var redos_count = this.CountRecentTransactions(TransactionType.Redo);
                Func<DBTransaction<T>, bool> matcher = x => x.TransactionType == TransactionType.Undo && x.Active == true;
                var undos_count = this.CountRecentTransactions(matcher, this.Transactions_DB.Skip(redos_count * 2));
                return undos_count > 0;
            }
        }

        /// <summary>
        /// Gets the current DB Version
        /// - Any DB newer than this value is considered too new to handle.
        /// - has internal set method so that Serializer can access it.
        /// </summary>
        [JsonProperty]
        public float DBVersion { get; internal set; }

        /// <summary>
        /// Gets the filename/path that is used to cache the collection in
        /// </summary>
        public string Filename { get; }

        /// <summary>
        /// Gets the method that is used to migrate old versions of the db
        /// </summary>
        protected Func<DBMigrationParameters, JToken> Migrator { get; }

        
        //protected string SerializeData
        //{
        //    get
        //    {
        //        lock (Locker)
        //        {
        //            // TODO: compress https://dotnet-snippets.de/snippet/strings-komprimieren-und-dekomprimieren/1058
        //            return JsonConvert.SerializeObject(this, new DataBaseSerializer<T>());
        //        }
        //    }
        //}

        /// <summary>
        /// Gets or sets the databse for caching transactions for undo/redo.
        /// </summary>
        private ObservableCollection<DBTransaction<IDatabaseObject>> Transactions_DB { get; set; }
        #endregion

        #region public methods   
        /// <summary>
        /// Helper method determining if there is a conflict between changes to two different db's based on both db's changing the same item (determined by ID).
        /// </summary>
        /// <param name="db1">first db</param>
        /// <param name="db2">second db</param>
        /// <returns>true if conflict</returns>
        public static bool ConflictIfSameItemChangedById(DataBase<T> db1, DataBase<T> db2)
        {
            bool result = false;

            // any transaction in db1 has the same item db as any tranasction in db2
            result = db1.Transactions_DB.Any(x => db2.Transactions_DB.Any(y => x.Item_ID == y.Item_ID));
            return result;
        }

        /// <summary>
        /// Clear the mutex when a using statement leaves scope (or destructor is called)
        /// </summary>
        public void Dispose()
        {
            if (this.mut != null && this.mut.WaitOne(TimeSpan.FromSeconds(5), false))
            {
                this.mut.ReleaseMutex();
                this.mut.Close();
                this.mut = null;
            }
        }

        /// <summary>
        /// Undo last undoable transaction and raise property changed events on changed item.
        /// TODO: make sure to use locker everywhere that it is messing with the data
        /// </summary>
        public void Undo()
        {
            // inside mutex; however, not in creation, so normal catch/dispose methods should clear mutex
            if (!this.CanUndo)
            {
                throw new DBCannotUndoException("Cannot undo at this time");
            }

            // get last transaction
            DBTransaction<T> last_transaction = this.GetLastTransaction(TransactionType.Undo, x => x.Active == true);

            T transactedItem = null;

            // get the mutex
            lock (Locker)
            {
                // deregister Changed handler
                this.CollectionChanged -= this.DataBase_CollectionChanged;
                bool registerItem = true;
                DBTransaction<T> undoTransaction;
                try
                {
                    if (last_transaction.TransactionType == TransactionType.Modify)
                    {
                        // get item from last transaction
                        transactedItem = this.FirstOrDefault(x => x.ID == last_transaction.Item_ID);
                        if (transactedItem == null)
                        {
                            throw new DBCannotUndoException(string.Format("Failed to find item with ID {1} to undo property {0}", last_transaction.Changed_property, last_transaction.Item_ID));
                        }

                        transactedItem.PropertyChangedExtended -= this.DataBaseItem_PropertyChanged;

                        SetProperty(last_transaction, transactedItem);

                        // Create Undo transaction                        
                        undoTransaction = new DBTransaction<T>()
                        {
                            TransactionType = TransactionType.Undo,
                            Item_ID = transactedItem.ID,
                            Transacted_item = null,
                            Property_old = last_transaction.Property_new,
                            Property_new = last_transaction.Property_old,
                            Changed_property = last_transaction.Changed_property
                        };
                    }
                    else if (last_transaction.TransactionType == TransactionType.Add)
                    {
                        transactedItem = this.FirstOrDefault(x => x.ID == last_transaction.Item_ID);
                        if (transactedItem == null)
                        {
                            throw new DBCannotUndoException(string.Format("Failed to find item with ID {0} to remove", last_transaction.Item_ID));
                        }

                        transactedItem.PropertyChangedExtended -= this.DataBaseItem_PropertyChanged;
                        registerItem = false;
                        if (last_transaction.Transacted_item == null)
                        {
                            throw new DBCannotUndoException();
                        }

                        this.Remove(transactedItem);
                        undoTransaction = new DBTransaction<T>()
                        {
                            TransactionType = TransactionType.Undo,
                            Item_ID = transactedItem.ID,
                            Transacted_item = transactedItem,
                            Property_old = null,
                            Property_new = null,
                            Changed_property = DBTransaction<T>.ItemRemovedConstKey
                        };
                    }
                    else if (last_transaction.TransactionType == TransactionType.Delete)
                    {
                        if (last_transaction.Transacted_item == null)
                        {
                            throw new DBCannotUndoException();
                        }

                        transactedItem = last_transaction.Transacted_item;
                        if (transactedItem == null)
                        {
                            throw new DBCannotUndoException(string.Format("Failed to find item to re-add", last_transaction.Changed_property, last_transaction.Item_ID));
                        }

                        transactedItem.PropertyChangedExtended -= this.DataBaseItem_PropertyChanged;

                        this.Add(transactedItem);

                        undoTransaction = new DBTransaction<T>()
                        {
                            TransactionType = TransactionType.Undo,
                            Item_ID = transactedItem.ID,
                            Transacted_item = last_transaction.Transacted_item,
                            Property_old = null,
                            Property_new = null,
                            Changed_property = DBTransaction<T>.ItemAddConstKey
                        };
                    }
                    else if (last_transaction.TransactionType == TransactionType.Redo)
                    {
                        // TODOne: if it was a REDO - there is more involved
                        if (last_transaction.Changed_property == DBTransaction<T>.ItemRemovedConstKey)
                        {
                            transactedItem = last_transaction.Transacted_item;
                            this.Add(transactedItem);
                            undoTransaction = new DBTransaction<T>
                            {
                                Item_ID = transactedItem.ID,
                                Changed_property = DBTransaction<T>.ItemAddConstKey,
                                TransactionType = TransactionType.Undo
                            };
                        }
                        else if (last_transaction.Changed_property == DBTransaction<T>.ItemAddConstKey)
                        {
                            transactedItem = last_transaction.Transacted_item;
                            if (transactedItem == null)
                            {
                                throw new DBCannotUndoException(string.Format("Failed to find item with ID {1} to redo property {0}", last_transaction.Changed_property, last_transaction.Item_ID));
                            }

                            transactedItem.PropertyChangedExtended -= this.DataBaseItem_PropertyChanged;
                            this.Remove(transactedItem);
                            registerItem = false;
                            undoTransaction = new DBTransaction<T>
                            {
                                Changed_property = DBTransaction<T>.ItemRemovedConstKey,
                                Item_ID = transactedItem.ID,
                                Transacted_item = transactedItem,
                                TransactionType = TransactionType.Undo
                            };
                        }
                        else
                        {
                            transactedItem = this.FirstOrDefault(x => x.ID == last_transaction.Item_ID);
                            if (transactedItem == null)
                            {
                                throw new DBCannotUndoException(string.Format("Failed to load item with ID {0} to reset {1}", last_transaction.Transacted_item.ID, last_transaction.Changed_property));
                            }

                            transactedItem.PropertyChangedExtended -= this.DataBaseItem_PropertyChanged;

                            SetProperty(last_transaction, transactedItem);

                            undoTransaction = new DBTransaction<T>()
                            {
                                TransactionType = TransactionType.Undo,
                                Item_ID = last_transaction.Item_ID,
                                Transacted_item = null,
                                Changed_property = last_transaction.Changed_property,
                                Property_new = last_transaction.Property_old,
                                Property_old = last_transaction.Property_new
                            };
                        }
                    }
                    else
                    {
                        throw new DBCannotUndoException();
                    }

                    this.OnItemChanged(transactedItem.ID);

                    // store Undo transaction at start
                    last_transaction.Active = false;
                    this.Transactions_DB.Insert(0, undoTransaction);
                }
                finally
                {
                    // reregister changed handler
                    if (registerItem)
                    {
                        transactedItem.PropertyChangedExtended += this.DataBaseItem_PropertyChanged;
                    }

                    this.CollectionChanged += this.DataBase_CollectionChanged;

                    // update the DB on disk
                    this._cacheDB();
                }
            }

            this.PublicOnPropertyChanged(nameof(this.CanUndo));
            this.PublicOnPropertyChanged(nameof(this.CanRedo));
        }

        /// <summary>
        /// Reverse the last undo transaction
        /// </summary>
        public void Redo()
        {
            // inside mutex; however, not in creation, so normal catch/dispose methods should clear mutex
            if (!this.CanRedo)
            {
                throw new DBCannotRedoException("Cannot redo at this time");
            }

            DBTransaction<T> last_transaction = this.GetLastTransaction(TransactionType.Redo, x => x.Active == true);
            T transactedItem;

            // get the mutex
            lock (Locker)
            {
                // deregister Changed handler
                this.CollectionChanged -= this.DataBase_CollectionChanged;

                if (last_transaction.TransactionType != TransactionType.Undo)
                {
                    throw new DBCannotRedoException();
                }

                DBTransaction<T> redoTransaction;
                if (last_transaction.Changed_property == DBTransaction<T>.ItemAddConstKey)
                {
                    // get item from last transaction
                    transactedItem = this.FirstOrDefault(x => x.ID == last_transaction.Item_ID);
                    transactedItem.PropertyChangedExtended -= this.DataBaseItem_PropertyChanged;
                    if (transactedItem == null)
                    {
                        throw new DBCannotRedoException(string.Format("Failed to find item with ID {1} to redo property {0}", last_transaction.Changed_property, last_transaction.Item_ID));
                    }

                    this.Remove(transactedItem);
                    redoTransaction = new DBTransaction<T>
                    {
                        Changed_property = DBTransaction<T>.ItemRemovedConstKey,
                        Transacted_item = transactedItem,
                        Item_ID = transactedItem.ID,
                        TransactionType = TransactionType.Redo
                    };
                }
                else if (last_transaction.Changed_property == DBTransaction<T>.ItemRemovedConstKey)
                {
                    transactedItem = last_transaction.Transacted_item;
                    transactedItem.PropertyChangedExtended -= this.DataBaseItem_PropertyChanged;
                    if (transactedItem == null)
                    {
                        throw new DBCannotRedoException(string.Format("Failed to load item with ID {0} to re-add", last_transaction.Transacted_item.ID));
                    }

                    this.Add(transactedItem);

                    redoTransaction = new DBTransaction<T>
                    {
                        Changed_property = DBTransaction<T>.ItemAddConstKey,
                        Transacted_item = transactedItem,
                        Item_ID = transactedItem.ID,
                        TransactionType = TransactionType.Redo
                    };
                }
                else
                {
                    transactedItem = this.FirstOrDefault(x => x.ID == last_transaction.Item_ID);
                    transactedItem.PropertyChangedExtended -= this.DataBaseItem_PropertyChanged;
                    if (transactedItem == null)
                    {
                        throw new DBCannotRedoException(string.Format("Failed to load item with ID {0} to reset {1}", last_transaction.Transacted_item.ID, last_transaction.Changed_property));
                    }

                    SetProperty(last_transaction, transactedItem);

                    redoTransaction = new DBTransaction<T>()
                    {
                        TransactionType = TransactionType.Redo,
                        Item_ID = last_transaction.Item_ID,
                        Transacted_item = null,
                        Changed_property = last_transaction.Changed_property,
                        Property_new = last_transaction.Property_old,
                        Property_old = last_transaction.Property_new
                    };
                }

                // store Redo transaction            
                this.Transactions_DB.Insert(0, redoTransaction);
                last_transaction.Active = false;

                // reregister changed handler
                transactedItem.PropertyChangedExtended += this.DataBaseItem_PropertyChanged;
                this.CollectionChanged += this.DataBase_CollectionChanged;

                this._cacheDB();
            }

            this.OnItemChanged(transactedItem.ID);
            this.PublicOnPropertyChanged(nameof(this.CanRedo));
            this.PublicOnPropertyChanged(nameof(this.CanUndo));
        }
        #endregion

        #region private methods

        /// <summary>
        /// Store the database to disk in <see cref="Filename"/>.
        /// </summary>
        private void _cacheDB()
        {
            lock (Locker)
            {
                //var json = this.SerializeData;
                //System.IO.File.WriteAllText(this.Filename, json);
                this.StorageStrategy._cacheDB(this);
            }
        }

        /// <summary>
        /// Use reflection to set reverse the last transaction on the transacted item
        /// </summary>
        /// <param name="last_transaction">what happened that we're changing back</param>
        /// <param name="transactedItem">the item to act on</param>
        private static void SetProperty(DBTransaction<T> last_transaction, T transactedItem)
        {
            // inside mutex; however, not in creation, so normal catch/dispose methods should clear mutex

            // TODO: redo with https://stackoverflow.com/a/13270302
            var properties = last_transaction.Changed_property.Split('.');
            object lastObject = transactedItem;
            System.Reflection.PropertyInfo currentProperty = null;

            foreach (var attribute in properties)
            {
                // have currentObject lag behind since it should reflect the object the last property is on
                if (currentProperty != null)
                {
                    lastObject = currentProperty.GetValue(lastObject);
                }

                // get the property information based on the type
                if (!attribute.Contains("["))
                {
                    currentProperty = lastObject.GetType().GetProperty(attribute, BindingFlags.Public | BindingFlags.Instance);
                }
                else
                {
                    if (!attribute.Contains("]"))
                    {
                        throw new DBCannotUndoException($"Property name {attribute} contains unmatched '['");
                    }

                    if (attribute.IndexOf('[') != 0)
                    {
                        var propertyName = attribute.Substring(0, attribute.IndexOf('['));
                        currentProperty = lastObject.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance); // get the dictionary property
                        lastObject = currentProperty.GetValue(lastObject); // get the dictionary object
                        if (lastObject == null)
                        {
                            throw new DBCannotUndoException($"Cannot access property {propertyName} on {lastObject}");
                        }
                    }

                    Type t = lastObject.GetType();
                    if (!t.IsGenericType || t.GetGenericTypeDefinition() != typeof(Dictionary<,>))
                    {
                        throw new DBCannotUndoException($"Property {lastObject} is not a dictionary, but was used with indexers");
                    }

                    var r = new Regex(@"\[.+\]");
                    Match m = r.Match(attribute);
                    if (!m.Success)
                    {
                        // possible??
                        throw new DBCannotUndoException($"Cannot undo property: {attribute}");
                    }

                    var keyType = t.GetGenericArguments()[0];
                    var valueType = t.GetGenericArguments()[1];

                    // store key without square brackets
                    var key = m.Value.Substring(1);
                    key = key.Substring(0, key.Length - 1);
                    var keyObject = Convert.ChangeType(key, keyType);

                    // currentProperty = Convert.ChangeType(lastObject, keyType);
                    var p1 = t.GetProperty("Item"); // get indexer property
                    lastObject = p1.GetValue(lastObject, new object[] { keyObject });
                    currentProperty = null;
                }
            }

            if (currentProperty == null)
            {
                throw new DBCannotUndoException($"Cannot access property {properties.First()} on {lastObject}");
            }

            // find the property type
            Type propertyType = currentProperty.PropertyType;

            // Convert.ChangeType does not handle conversion to nullable types
            //  if the property type is nullable, we need to get the underlying type of the property
            var targetType = IsNullableType(propertyType) ? Nullable.GetUnderlyingType(propertyType) : propertyType;

            // Returns an System.Object with the specified System.Type and whose value is
            //   equivalent to the specified object.
            object oldVal = null;
            if (targetType.IsEnum)
            {
                // need converter for int to enum
                oldVal = Enum.ToObject(targetType, last_transaction.Property_old);
            }
            else
            {
                oldVal = Convert.ChangeType(last_transaction.Property_old, targetType);
            }

            // Set the value of the property
            currentProperty.SetValue(lastObject, oldVal, null);
        }

        /// <summary>
        /// Determine if the objece is a nullable type
        /// </summary>
        /// <param name="type">the type in question</param>
        /// <returns>true if nullable</returns>
        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
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
        /// When the transaction stored in the Transactions_DB  is changed, cache it
        /// </summary>
        /// <param name="sender">this.transactions_db is expected</param>
        /// <param name="e">event args (ignored)</param>
        private void DataBase_TransactionsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // called on primary db
            lock (Locker)
            {
                this.StorageStrategy.cacheTransactions(Transactions_DB);
            }
        }

        /// <summary>
        /// When the collection is changed, cache the db and log the adds/removes/changes that occured in the transactions db to be able to undo/redo later
        /// </summary>
        /// <param name="sender">database that changed (should be this)</param>
        /// <param name="e">How the collection changed</param>
        private void DataBase_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            lock (Locker)
            {
                this._cacheDB();

                // cache transactions
                TransactionType transactionType;
                DBTransaction<T> transaction;
                System.Collections.IList changed = null;
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    transactionType = TransactionType.Add;
                    foreach (T item in e.NewItems)
                    {
                        // register for property change
                        item.PropertyChangedExtended += this.DataBaseItem_PropertyChanged;

                        // create add transaction
                        transaction = new DBTransaction<T>()
                        {
                            TransactionType = transactionType,
                            Item_ID = (item as T).ID,
                            Transacted_item = item as T
                        };
                        this.Transactions_DB.Insert(0, transaction);
                    }

                    changed = e.NewItems;
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
                {
                    transactionType = TransactionType.Modify;
                    foreach (T item in e.NewItems)
                    {
                        // register for property change
                        item.PropertyChangedExtended += this.DataBaseItem_PropertyChanged;

                        // create modify transaction
                        transaction = new DBTransaction<T>()
                        {
                            TransactionType = transactionType,
                            Item_ID = (item as T).ID,
                            Transacted_item = item as T
                        };
                        this.Transactions_DB.Insert(0, transaction);
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    // removing the item - mark a delete transaction
                    transactionType = TransactionType.Delete;
                    foreach (var item in e.OldItems)
                    {
                        transaction = new DBTransaction<T>()
                        {
                            TransactionType = transactionType,
                            Item_ID = (item as T).ID,
                            Transacted_item = item as T
                        };
                        this.Transactions_DB.Insert(0, transaction);
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    // reset the entire collection
                    this.Transactions_DB.Clear();
                }
                else
                {
                    // not sure, but if anything was added or modified, we want to log it.
                    if (e.NewItems != null)
                    {
                        transactionType = TransactionType.Unknown;
                        foreach (var item in this)
                        {
                            transaction = new DBTransaction<T>()
                            {
                                TransactionType = transactionType,
                                Item_ID = (item as T).ID,
                                Transacted_item = item as T
                            };
                            this.Transactions_DB.Insert(0, transaction);
                        }
                    }
                }
            }

            this.PublicOnPropertyChanged(nameof(this.CanUndo));
            this.PublicOnPropertyChanged(nameof(this.CanRedo));
        }

        /// <summary>
        /// When a property is changed on an item int the db, store the transaction information, and cache the db
        /// </summary>
        /// <param name="sender">the DB object (T) that was changed</param>
        /// <param name="e">PropertyChangedExtendedEventArgs to store transaction from</param>
        private void DataBaseItem_PropertyChanged(object sender, PropertyChangedExtendedEventArgs e)
        {
            this.ReleaseMutexOnError(() =>
            {
                lock (Locker)
                {
                    var item = sender as T;
                    if (item == null)
                    {
                        throw new Exception("Sender must be dbObject");
                    }

                    this._cacheDB();

                    // if not an undoable change, alert property changes and leave
                    if (!e.UndoableChange)
                    {
                        this.OnItemChanged(item);
                        this.PublicOnPropertyChanged(nameof(this.CanUndo));
                        this.PublicOnPropertyChanged(nameof(this.CanRedo));
                        return;
                    }

                    // else, store a modify with information about how to undo
                    var transaction = new DBTransaction<T>()
                    {
                        TransactionType = TransactionType.Modify,
                        Item_ID = item.ID,
                        Transacted_item = null,
                        Property_old = e.OldValue,
                        Property_new = e.NewValue,
                        Changed_property = e.PropertyName
                    };
                    this.Transactions_DB.Insert(0, transaction);
                    this.OnItemChanged(item);
                    this.PublicOnPropertyChanged(nameof(this.CanUndo));
                    this.PublicOnPropertyChanged(nameof(this.CanRedo));
                }
            });
        }

        /// <summary>
        /// Raise the ItemChanged event, passing in the itemChanged's id to handlers
        /// </summary>
        /// <param name="itemChanged">the object of type T (databaseObject) that changed.</param>
        private void OnItemChanged(T itemChanged)
        {
            this.ItemChanged?.Invoke(this, itemChanged?.ID);
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
        /// Count recent transactions that match the provided TransactionType
        /// </summary>
        /// <param name="transactionType">transaction type to count</param>
        /// <param name="list">(optional) the list to search (defaults to this)</param>
        /// <returns>the count of matches</returns>
        private int CountRecentTransactions(TransactionType transactionType, IEnumerable<DBTransaction<T>> list = null)
        {
            if (list == null)
            {
                list = this.Transactions_DB;
            }

            var first = list.FirstOrDefault() as DBTransaction<T>;
            if (first == null)
            {
                return 0;
            }

            int count = (first.TransactionType == transactionType) ?
                list.Select((item, index) => new { item, index })
                    .Where(x => (x.item.TransactionType != transactionType)) // find non matches
                    .Select(x => x.index)                                    // select the index
                    .FirstOrDefault()                                        // return first index
                : 0;
            return count;
        }

        /// <summary>
        /// Count recent transactions that match the predicate
        /// </summary>
        /// <param name="predicate">function that takes a transaction and returns if it matches or not</param>
        /// <param name="list">(optional) the list to search (defaults to this)</param>
        /// <returns>the count of matches</returns>
        private int CountRecentTransactions(Func<DBTransaction<T>, bool> predicate, IEnumerable<DBTransaction<T>> list = null)
        {
            if (list == null)
            {
                list = this.Transactions_DB;
            }

            var first = list.FirstOrDefault() as DBTransaction<T>;
            if (first == null)
            {
                return 0;
            }

            int count = predicate(first) ?
                list.Select((item, index) => new { item, index })
                    .Where(x => !predicate(x.item)) // find non matching
                    .Select(x => x.index)           // select its index
                    .FirstOrDefault()               // get first value (index of first non-match)
                : 0;
            return count;
        }

        /// <summary>
        /// Count the number of transactions at the start of the transactions db that are of a type in transactionTypes
        /// </summary>
        /// <param name="transactionTypes">transaction types to count</param>
        /// <param name="list">(optional) list of transactions to count in (uses this if not provided).</param>
        /// <returns>the count of transactions</returns>
        private int CountRecentTransactions(List<TransactionType> transactionTypes, IEnumerable<DBTransaction<T>> list = null)
        {
            return this.ReleaseMutexOnError<int>(() =>
            {
                if (list == null)
                {
                    list = this.Transactions_DB;
                }

                var first = list.FirstOrDefault() as DBTransaction<T>;
                if (first == null)
                {
                    return 0;
                }

                int count = transactionTypes.Contains(first.TransactionType) ?
                    list.Select((item, index) => new { item, index })
                        .Where(x => !transactionTypes.Contains(x.item.TransactionType)).Select(x => x.index).FirstOrDefault()
                    : 0;
                return count;
            });
        }

        /// <summary>
        /// Read in the file
        /// </summary>
        /// <param name="file">filename or path to load</param>
        /// <param name="registerItemsForPropertyChange">If true, DataBaseItem_PropertyChanged is registered for each item's property changed extended</param>
        private void LoadFile(string file, bool registerItemsForPropertyChange)
        {
            this.ReleaseMutexOnError(() =>
            {
                var adapted = this.StorageStrategy._loadDB(file);
                if (adapted == null)
                {
                    return;
                }

                if (adapted.DBVersion > this.DBVersion)
                {
                    throw new DBCreationException($"Cannot load db of version {adapted.DBVersion}. Current version is only {this.DBVersion}");
                }

                // if not the current version, try to update.
                if (adapted.DBVersion != this.DBVersion && adapted.DBVersion >= this.MinimumCompatibleVersion)
                {
                    this.StorageStrategy._migrate(adapted.DBVersion, this.DBVersion);
                }

                // if new enough, and not too new,
                if (adapted.DBVersion >= this.MinimumCompatibleVersion && adapted.DBVersion <= this.DBVersion)
                {
                    // parse and load
                    foreach (var item in adapted)
                    {
                        this.Add(item);
                        if (registerItemsForPropertyChange)
                        {
                            item.PropertyChangedExtended += this.DataBaseItem_PropertyChanged;
                        }
                    }

                }
                else
                {
                    throw new DBCreationException($"DB version {adapted.DBVersion} too old. Oldest supported db version is {MinimumCompatibleVersion}");
                }
            });
            //    if (System.IO.File.Exists(file))
            //    {
            //        var json = this.StorageStrategy.(file);

            //        if (json.Length > 0)
            //        {
            //            var adapted = this.StorageStrategy._loadDB(this.Filename); // JsonConvert.DeserializeObject<DataBase<T>>(json, new DataBaseSerializer<T>());

            
        }

        /// <summary>
        /// Get last transaction skipping all transaction with a type in notTransactionType and matches matcher
        /// </summary>
        /// <param name="notTransactionType">List of transaction types to skip</param>
        /// <param name="matcher">ethod to determine a match</param>
        /// <returns>last transaction skipping all transaction with a type in notTransactionType and matches matcher</returns>
        private DBTransaction<T> GetLastTransaction(TransactionType notTransactionType, Func<DBTransaction<T>, bool> matcher)
        {
            return this.ReleaseMutexOnError<DBTransaction<T>>(() =>
            {
                DBTransaction<T> last_transaction = null;
                IEnumerable<DBTransaction<T>> temp_list = this.Transactions_DB;
                do
                {
                    bool first_matches = false;

                    // is first in list active?
                    if (temp_list != null & temp_list.Count() > 0)
                    {
                        first_matches = matcher(temp_list.First());
                    }

                    if (temp_list == null || temp_list.Count() == 0)
                    {
                        // ran out of options
                        throw new DBCannotUndoException("Cannot find transaction");
                    }

                    if (temp_list.FirstOrDefault()?.TransactionType == notTransactionType)
                    {
                        // transaction type to skip past
                        var count = this.CountRecentTransactions(notTransactionType, temp_list);
                        temp_list = temp_list.Skip(count * 2);
                    }
                    else if (first_matches)
                    {
                        last_transaction = temp_list.FirstOrDefault();
                    }
                    else
                    {
                        temp_list = temp_list.Skip(1);
                    }
                }
                while (last_transaction == null || last_transaction.TransactionType == notTransactionType);
                return last_transaction;
            });
        }

        /// <summary>
        /// Get the latest transaction that is not in notTransationTypes
        /// </summary>
        /// <param name="notTransactionTypes">List of transaction types to skip</param>
        /// <returns> the latest transaction that is not in notTransationTypes</returns>
        private DBTransaction<T> GetLastTransaction(List<TransactionType> notTransactionTypes)
        {
            return this.ReleaseMutexOnError<DBTransaction<T>>(() =>
            {
                DBTransaction<T> last_transaction = null;
                IEnumerable<DBTransaction<T>> temp_list = this.Transactions_DB;
                do
                {
                    if (temp_list == null || temp_list.Count() == 0)
                    {
                        throw new DBCannotUndoException("Cannot find transaction");
                    }

                    var first_transaction = temp_list.FirstOrDefault().TransactionType;
                    if (notTransactionTypes.Contains(first_transaction))
                    {
                        var count = this.CountRecentTransactions(notTransactionTypes, temp_list);
                        temp_list = temp_list.Skip(count * 2);
                    }
                    else
                    {
                        last_transaction = temp_list.FirstOrDefault();
                    }
                }
                while (last_transaction == null || notTransactionTypes.Contains(last_transaction.TransactionType));
                return last_transaction;
            });
        }

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

        /// <summary>
        /// The information provided to a migration method
        /// </summary>
        public struct DBMigrationParameters
        {
            /// <summary>
            /// The version of DB that is loaded
            /// </summary>
            public float OldVersion;

            /// <summary>
            /// The version of DB that is known to this compile
            /// </summary>
            public float TargetVersion;

            /// <summary>
            /// The JSON that is being loaded from the old version
            /// </summary>
            public JToken Collection;
        }
    }
}
