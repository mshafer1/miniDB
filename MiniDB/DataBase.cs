using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiniDB
{
    public class DataBase<T> : ObservableCollection<T>, IDisposable where T : IDatabaseObject
    {
        #region fields

        private readonly IStorageStrategy<T> storageStrategy;

        /// <summary>
        /// lock object - this is used to lock the current database from attempting to edit the data at the same time.
        /// </summary>
        protected static readonly object Locker = new object();

        /// <summary>
        /// http://www.albahari.com/threading/part2.aspx#_Mutex
        /// Create mutex in constructor - name it so that only one instance of db class can be accessing file
        ///  - this allows for multiple instances of a DB, but only one of a given type accessing a given file
        /// </summary>
        private Mutex mut = null;

        private readonly ObservableCollection<DBTransaction<T>> Transactions_DB;

        #endregion

        #region Constructors

        public DataBase(string filename, IStorageStrategy<T> storageStrategy)
        {
            
            this.Filename = filename;
            this.Filename = Path.GetFullPath(this.Filename); // use the full system path - especially for mutex to know if it needs to lock that file or not

            this.storageStrategy = storageStrategy;

            lock (Locker)
            {
                Type t = typeof(T);
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(DBTransaction<>))
                {
                    // prevent recursion - not inside mutex yet, so don't need to release if thrown
                    throw new DBCreationException("Cannot create databse of DBTransaction<T>");
                }

                this.getMutex();

                string transactionFilename = string.Format(@"{0}\transactions_{1}.data", Path.GetDirectoryName(this.Filename), Path.GetFileName(this.Filename));
                this.Transactions_DB = this.storageStrategy._getTransactionsCollection(); //this._getTransactionsDB(transactionFilename);
                this.Transactions_DB.CollectionChanged += this.DataBase_TransactionsChanged;
            }
        }

        private void DataBase_TransactionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        internal DataBase()
        {
            // NOOP
            // TODO: acquire mutex??
        }
        
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
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

        #region fields

        public string Filename { get; }
        public float DBVersion { get; internal set; }
        

        #endregion

        #region methods

        #region API

        #endregion

        #region helper methods

        private void getMutex()
        {
            string mutex_file_path = this.Filename.Replace("\\", "_").Replace("/", "_");
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
        }

        #endregion

        #endregion
    }
}
