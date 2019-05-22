using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniDB;
using MiniDB.Transactions;

namespace DbXunitTests
{
    class NullWriterStorageStrategy : MiniDB.IStorageStrategy
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

        public void _cacheTransactions(ObservableCollection<IDBTransaction> dBTransactions, string filename)
        {
            // NOOP
            this.OnTransactionsWrite(dBTransactions);
        }

        public void _cacheDB(DataBase db)
        {
            // NOOP
            this.OnMainWrite(db);
        }

        public ObservableCollection<IDBTransaction> _getTransactionsCollection(string filename)
        {
            return new ObservableCollection<IDBTransaction>();
        }

        public DataBase _loadDB(string filename)
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

        public void _migrate(float oldVersion, float newVersion)
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
