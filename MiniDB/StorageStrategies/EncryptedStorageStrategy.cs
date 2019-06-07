using System;
using System.Collections.ObjectModel;

using MiniDB.Interfaces;
using MiniDB.Transactions;

namespace MiniDB
{
    public class EncryptedStorageStrategy : IStorageStrategy
    {
        public void CacheTransactions(ObservableCollection<IDBTransaction> dBTransactions, string filename)
        {
            throw new NotImplementedException();
        }

        public void CacheDB(DataBase db)
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<IDBTransaction> GetTransactionsCollection(string filename)
        {
            throw new NotImplementedException();
        }

        public DataBase LoadDB(string filename)
        {
            throw new NotImplementedException();
        }

        public void Migrate(float oldVersion, float newVersion)
        {
            throw new NotImplementedException();
        }
    }
}
