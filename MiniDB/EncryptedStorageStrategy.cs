﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    class EncryptedStorageStrategy<T> : IStorageStrategy<T> where T : DatabaseObject
    {
        public void cacheTransactions(ObservableCollection<DBTransaction<T>> dBTransactions)
        {
            throw new NotImplementedException();
        }

        public void _cacheDB(DataBase<T> db)
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<DBTransaction<T>> _getTransactionsCollection()
        {
            throw new NotImplementedException();
        }

        public DataBase<DBTransaction<T>> _getTransactionsDB(string transactions_filename)
        {
            throw new NotImplementedException();
        }

        public DataBase<T> _loadDB()
        {
            throw new NotImplementedException();
        }

        public void _migrate(float oldVersion, float newVersion)
        {
            throw new NotImplementedException();
        }
    }
}
