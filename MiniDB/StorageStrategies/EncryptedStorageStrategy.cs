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
        public void cacheTransactions(ObservableCollection<DBTransaction> dBTransactions)
        {
            throw new NotImplementedException();
        }

        public void _cacheDB(DataBase db)
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<DBTransaction> _getTransactionsCollection(string filename)
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
