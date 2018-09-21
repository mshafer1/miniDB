using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace MiniDB
{
    /// <summary>
    /// An implementation of the DataBase Template of type T that uses a Rijndael Managed algorithm to store and reload the data from disk to hide it from prying eyes.
    /// </summary>
    /// <typeparam name="T">Class with DatabaseObject base to make a persistent, encrypted, observable collection of</typeparam>
    public class EncryptedDataBase<T> : DataBase<T> where T : DatabaseObject
    {
       #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptedDataBase{T}" /> class.
        /// Create instance of database - if file exists, load collection from it; else, create new empty collection
        /// </summary>
        /// <param name="filename">The filename or path to store the collection in</param>
        /// <param name="dataBaseVersion">The current version of the database - if unsure what to use, put 0.1 for now</param>
        /// <param name="minimumCompatibleVersion">The mimum compatible version - if unsure what to use, put 0.1 for now</param>
        public EncryptedDataBase(string filename, float dataBaseVersion, float minimumCompatibleVersion) : base(filename, dataBaseVersion, minimumCompatibleVersion, DBStorageStrategies<T>.Encrypted) // TODO: add storage strategy
        {
            // NOOP
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptedDataBase{T}" /> class.
        /// Create instance of database - if file exists, load collection from it; else, create new empty collection
        /// </summary>
        /// <param name="filename">The filename or path to store the collection in</param>
        /// <param name="dataBaseVersion">The current version of the database - if unsure what to use, put 0.1 for now</param>
        /// <param name="minimumCompatibleVersion">The mimum compatible version - if unsure what to use, put 0.1 for now</param>
        /// <param name="base_case">Parameter to force calling the base case</param>
        private EncryptedDataBase(string filename, float dataBaseVersion, float minimumCompatibleVersion, bool base_case) : base(filename, dataBaseVersion, minimumCompatibleVersion, DBStorageStrategies<T>.Encrypted, base_case) // TODO: add storage strategy
        {
            // NOOP
        }

        internal EncryptedDataBase() : base()
        {
            // NOOP
        }
        #endregion

        

        #region overrides
        ///// <summary>
        ///// Decrypts and loads the transactions db
        ///// </summary>
        ///// <param name="transactions_filename">the filename/path that the db is stored in</param>
        ///// <returns>new DataBase of <see cref="DBTransaction{T}" /></returns>
        //protected override DataBase<DBTransaction<T>> _getTransactionsDB(string transactions_filename)
        //{
        //    return new EncryptedDataBase<DBTransaction<T>>(transactions_filename, this.DBVersion, this.MinimumCompatibleVersion, true);
        //}

        ///// <summary>
        ///// Overides the default ReadFile to decrypt the file on load
        ///// </summary>
        ///// <param name="filename">The filename or path to read</param>
        ///// <returns>Json searlized <see cref="DataBase{T}" /> from file.</returns>
        //protected override string _readFile(string filename)
        //{
        //    if (System.IO.File.Exists(filename))
        //    {
        //        var json = this.DecryptFile(filename);
        //        return json;
        //    }

        //    return string.Empty;
        //}
        #endregion

        #region helper methods
        

        
        #endregion
    }
}
