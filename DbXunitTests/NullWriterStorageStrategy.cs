using System.Collections.Generic;
using System.Collections.ObjectModel;

using MiniDB;
using MiniDB.Transactions;

using MiniDB.Interfaces;

namespace DbXunitTests
{
    class NullWriterStorageStrategy : MiniDB.Interfaces.IStorageStrategy
    {
        public delegate void WriteMessageHandler(IEnumerable<IDBObject> data);
        public delegate void WriteTransactionHandler(IEnumerable<IDBTransaction> data);

        public event WriteMessageHandler WroteMain;
        public event WriteTransactionHandler WroteTransactions;

        public NullWriterStorageStrategy(): base()
        {
            this.WroteFlag = false;
            this.WroteTransactionsFlag = false;
        }

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
            if(this.dBObjects == null)
            {
                return result;
            }

            foreach(var item in this.dBObjects)
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

        public IEnumerable<IDBObject> dBObjects { get; set; }

        public bool WroteFlag { get; private set; }
        public bool WroteTransactionsFlag { get; private set; }
    }
}
