using System.Collections.ObjectModel;

using MiniDB.Transactions;

namespace MiniDB.Interfaces
{
    public interface IStorageStrategy : ITransactionStorageStrategy
    {
        /// <summary>
        /// load the DB
        /// </summary>
        /// <param name="filename">File to load</param>
        /// <returns>An instance of <see cref="DataBase{T}" /> from file.</returns>
        DataBase _loadDB(string filename);

        /// <summary>
        /// Method to store in file;
        /// </summary>
        void _cacheDB(DataBase db);

        /// <summary>
        /// Method to migrate files stored in this storage strategy
        /// </summary>
        /// <param name="oldVersion"></param>
        /// <param name="newVersion"></param>
        void _migrate(float oldVersion, float newVersion);
    }
}