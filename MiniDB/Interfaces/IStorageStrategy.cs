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
        DataBase LoadDB(string filename);

        /// <summary>
        /// Method to store in file;
        /// </summary>
        /// <param name="db">Database to store</param>
        void CacheDB(DataBase db);

        /// <summary>
        /// Method to migrate files stored in this storage strategy
        /// </summary>
        /// <param name="oldVersion">Version from</param>
        /// <param name="newVersion">Version to</param>
        void Migrate(float oldVersion, float newVersion);
    }
}