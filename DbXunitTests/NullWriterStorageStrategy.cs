using System.Collections.Generic;
using System.Collections.ObjectModel;

using MiniDB;
using MiniDB.Interfaces;
using MiniDB.Transactions;

namespace DbXunitTests
{
    internal class NullWriterStorageStrategy : IStorageStrategy
    {
        public NullWriterStorageStrategy()
            : base()
        {
            this.WroteFlag = false;
            this.WroteTransactionsFlag = false;
        }

        public delegate void WriteMessageHandler(IEnumerable<IDBObject> data);

        public delegate void WriteTransactionHandler(IEnumerable<IDBTransaction> data);

        #region Events
        public event WriteMessageHandler WroteMain;

        public event WriteTransactionHandler WroteTransactions;
        #endregion

        #region Properties
        public IEnumerable<IDBObject> DBObjects { get; set; }

        public bool WroteFlag { get; private set; }

        public bool WroteTransactionsFlag { get; private set; }
        #endregion

        #region API
        public void CacheTransactions(ObservableCollection<IDBTransaction> dBTransactions, string filename)
        {
            // NOOP
            this.OnTransactionsWrite(dBTransactions);
        }

        public void CacheDB(DataBase db)
        {
            // NOOP
            this.OnMainWrite(db);
        }

        public ObservableCollection<IDBTransaction> GetTransactionsCollection(string filename)
        {
            return new ObservableCollection<IDBTransaction>();
        }

        public DataBase LoadDB(string filename)
        {
            var result = new DataBase("blah", 1, 1);
            if (this.DBObjects == null)
            {
                return result;
            }

            foreach (var item in this.DBObjects)
            {
                result.Add(item);
            }

            return result;
        }

        public void Migrate(float oldVersion, float newVersion)
        {
            // NOOP
        }

        public void ClearWroteFlags()
        {
            this.WroteFlag = false;
            this.WroteTransactionsFlag = false;
        }
        #endregion

        #region HelperMethods
        private void OnMainWrite(IEnumerable<IDBObject> data)
        {
            this.WroteFlag = true;
            this.WroteMain?.Invoke(data);
        }

        private void OnTransactionsWrite(IEnumerable<IDBTransaction> data)
        {
            this.WroteTransactionsFlag = true;
            this.WroteTransactions?.Invoke(data);
        }
        #endregion
    }
}
