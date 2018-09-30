using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        ObservableCollection<DBTransaction<T>> _getTransactionsCollection();

        void cacheTransactions(ObservableCollection<DBTransaction<T>> dBTransactions);

        /// <summary>
        /// load the DB
        /// </summary>
        /// <returns>An instance of <see cref="DataBase{T}" /> from file.</returns>
        DataBase<T> _loadDB();

        /// <summary>
        /// Method to store in file;
        /// </summary>
        void _cacheDB(DataBase<T> db);

        /// <summary>
        /// Method to migrate files stored in this storage strategy
        /// </summary>
        /// <param name="oldVersion"></param>
        /// <param name="newVersion"></param>
        void _migrate(float oldVersion, float newVersion);
    }
}
