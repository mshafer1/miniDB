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
        }

        public void cacheTransactions(ObservableCollection<DBTransaction> dBTransactions)
        {
           // NOOP
        }

        public void _cacheDB(DataBase db)
        {
            // NOOP
            this.WroteFlag = true;
        }

        public ObservableCollection<DBTransaction> _getTransactionsCollection(string filename)
        {
            return new ObservableCollection<DBTransaction>();
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

        public void ClearWroteFlag()
        {
            this.WroteFlag = false;
        }
    }
}
