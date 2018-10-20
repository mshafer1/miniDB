using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniDB;

namespace DbXunitTests
{
    class NullWriterStorageStrategy : MiniDB.IStorageStrategy
    {
        public NullWriterStorageStrategy(): base()
        {
            this.WroteFlag = false;
            this.WroteTransactionsFlag = false;
        }

        public void cacheTransactions(ObservableCollection<IDBTransaction> dBTransactions)
        {
            // NOOP
            this.WroteTransactionsFlag = true;
        }

        public void _cacheDB(DataBase db)
        {
            // NOOP
            this.WroteFlag = true;
        }

        public ObservableCollection<IDBTransaction> _getTransactionsCollection(string filename)
        {
            return new ObservableCollection<IDBTransaction>();
        }

        public DataBase _loadDB(string filename)
        {
            return new DataBase("blah", 1, 1);
        }

        public void _migrate(float oldVersion, float newVersion)
        {
            // NOOP
        }

        public bool WroteFlag { get; private set; }
        public bool WroteTransactionsFlag { get; private set; }

        public void ClearWroteFlags()
        {
            this.WroteFlag = false;
            this.WroteTransactionsFlag = false;
        }
    }
}
