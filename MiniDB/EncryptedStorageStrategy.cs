using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    class EncryptedStorageStrategy<T> : IStorageStrategy<T> where T : DatabaseObject
    {
        public static void _cacheDB(DataBase<T> db)
        {
            throw new NotImplementedException();
        }

        public static DataBase<DBTransaction<T>> _getTransactionsDB(string transactions_filename)
        {
            throw new NotImplementedException();
        }

        public static DataBase<T> _loadDB(string filename)
        {
            throw new NotImplementedException();
        }

        public static void _migrate(string filename, float oldVersion, float newVersion)
        {
            throw new NotImplementedException();
        }
    }
}
