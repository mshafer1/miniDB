using MiniDB.Transactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    public class EncryptedStorageStrategy : IStorageStrategy
    {
        public void _cacheTransactions(ObservableCollection<IDBTransaction> dBTransactions, string filename)
        {
            throw new NotImplementedException();
        }

        public void _cacheDB(DataBase db)
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<IDBTransaction> _getTransactionsCollection(string filename)
        {
            throw new NotImplementedException();
        }

        public DataBase _loadDB(string filename)
        {
            throw new NotImplementedException();
        }

        public void _migrate(float oldVersion, float newVersion)
        {
            throw new NotImplementedException();
        }
    }
}
