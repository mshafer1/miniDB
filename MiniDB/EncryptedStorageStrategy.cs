using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    class EncryptedStorageStrategy<T> : IStorageStrategy<T> where T : IDatabaseObject
    {
        public void cacheTransactions(ObservableCollection<DBTransaction<T>> dBTransactions)
        {
            throw new NotImplementedException();
        }

        public void _cacheDB<T2>(DataBase<T2> db) where T2 : DatabaseObject
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<DBTransaction<T>> _getTransactionsCollection()
        {
            throw new NotImplementedException();
        }

        public DataBase<T2> _loadDB<T2>() where T2 : DatabaseObject
        {
            throw new NotImplementedException();
        }

        public void _migrate(float oldVersion, float newVersion)
        {
            throw new NotImplementedException();
        }
    }
}
