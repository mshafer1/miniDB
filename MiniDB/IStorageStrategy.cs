using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IStorageStrategy<T> where T : IDatabaseObject
    {
        /// <summary>
        /// load the transactions db
        /// </summary>
        /// <param name="transactions_filename">the filename/path that the db is stored in</param>
        /// <returns>new DataBase of <see cref="DBTransaction{T}" /></returns>
        DataBase<DBTransaction<IDatabaseObject>> _getTransactionsDB(string transactions_filename, float dbVersion, float minimumCompatibleVersion, IStorageStrategy<DBTransaction<IDatabaseObject>> storageStrategy);

        /// <summary>
        /// load the DB
        /// </summary>
        /// <param name="filename">The filename or path to read</param>
        /// <returns>An instance of <see cref="DataBase{T}" /> from file.</returns>
        DataBase<T> _loadDB(string filename);

        /// <summary>
        /// Method to store in file;
        /// </summary>
        void _cacheDB(DataBase<T> db);

        /// <summary>
        /// Method to migrate files stored in this storage strategy
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="oldVersion"></param>
        /// <param name="newVersion"></param>
        void _migrate(string filename, float oldVersion, float newVersion);
    }
}
