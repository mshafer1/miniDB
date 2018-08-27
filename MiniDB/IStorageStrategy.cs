﻿using System;
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
    public interface IStorageStrategy<T> where T : DatabaseObject
    {
        /// <summary>
        /// load the transactions db
        /// </summary>
        /// <param name="transactions_filename">the filename/path that the db is stored in</param>
        /// <returns>new DataBase of <see cref="DBTransaction{T}" /></returns>
        DataBase<DBTransaction<T>> _getTransactionsDB(string transactions_filename);

        /// <summary>
        /// load the DB
        /// </summary>
        /// <param name="filename">The filename or path to read</param>
        /// <returns>An instance of <see cref="DataBase{T}" /> from file.</returns>
        DataBase<T> _loadDB(string filename);

        /// <summary>
        /// Method to store in file;
        /// </summary>
        void _cacheDB();
    }
}
