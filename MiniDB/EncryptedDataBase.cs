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
        /// <param name="dataBaseVersion">The current version of the database (stored only to one decimal place and max value of 25.5 - if unsure what to use, put 0.1 for now</param>
        /// <param name="minimumCompatibleVersion">The mimum compatible version - if unsure what to use, put 0.1 for now</param>
        public EncryptedDataBase(string filename, float dataBaseVersion, float minimumCompatibleVersion) : base(filename, dataBaseVersion, minimumCompatibleVersion)
        {
            // NOOP
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptedDataBase{T}" /> class.
        /// Create instance of database - if file exists, load collection from it; else, create new empty collection
        /// </summary>
        /// <param name="filename">The filename or path to store the collection in</param>
        /// <param name="dataBaseVersion">The current version of the database (stored only to one decimal place and max value of 25.5 - if unsure what to use, put 0.1 for now</param>
        /// <param name="minimumCompatibleVersion">The mimum compatible version - if unsure what to use, put 0.1 for now</param>
        /// <param name="base_case">Parameter to force calling the base case</param>
        private EncryptedDataBase(string filename, float dataBaseVersion, float minimumCompatibleVersion, bool base_case) : base(filename, dataBaseVersion, minimumCompatibleVersion, base_case)
        {
            // NOOP
        }
        #endregion

        #region properties
        /// <summary>
        /// Gets the private key used in the encryption/decryption
        /// </summary>
        internal byte[] Key
        {
            get
            {
                return HardwareID;
            }
        }

        /// <summary>
        /// Gets a 16 byte hardware specific ID - used in encrypting/decrypting the database
        /// </summary>
        private static byte[] HardwareID
        {
            get => DBHardwareID.IDValueBytes().Take(16).ToArray();
        }
        #endregion

        #region overrides
        /// <summary>
        /// Decrypts and loads the transactions db
        /// </summary>
        /// <param name="transactions_filename">the filename/path that the db is stored in</param>
        /// <returns>new DataBase of <see cref="DBTransaction{T}" /></returns>
        protected override DataBase<DBTransaction<T>> _getTransactionsDB(string transactions_filename)
        {
            return new EncryptedDataBase<DBTransaction<T>>(transactions_filename, this.DBVersion, this.MinimumCompatibleVersion, true);
        }

        /// <summary>
        /// Overides the default ReadFile to decrypt the file on load
        /// </summary>
        /// <param name="filename">The filename or path to read</param>
        /// <returns>Json searlized <see cref="DataBase{T}" /> from file.</returns>
        protected override string _readFile(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                var json = this.DecryptFile(filename);
                return json;
            }

            return string.Empty;
        }

        /// <summary>
        /// Override the base cacheDB method to store an encrypted version instead.
        /// </summary>
        protected override void _cacheDB()
        {
            lock (DataBase<T>.Locker)
            {
                // store local version
                this.EncryptFile();
            }
        }
        #endregion

        #region helper methods
        /// <summary>
        /// Helper method to load in the file, decrypt the binary blob, and return valid data
        /// </summary>
        /// <param name="filename">the file or path to load</param>
        /// <returns>DB JSON representation from binary blob in file</returns>
        private string DecryptFile(string filename)
        {
            // Create a new instance of the RijndaelManaged class  
            //  and decrypt the stream.  
            RijndaelManaged rindaelManagedCrypto = new RijndaelManaged();
            byte[] initializationVector = new byte[this.Key.Length];
            using (System.IO.FileStream fileStream = new FileStream(this.Filename, FileMode.Open))
            {
                var fileVersion = (short)fileStream.ReadByte();

                // TODO: implement overidable call back for migrating versions in both this db and in the base db.
                switch (fileVersion)
                {
                    case 12:
                    case 11:
                    case 10:
                        fileStream.Read(initializationVector, 0, initializationVector.Length);
                        fileStream.Seek(initializationVector.Length + 1, SeekOrigin.Begin); // + 1 for version byte
                        using (CryptoStream cryptoStream = new CryptoStream(fileStream, rindaelManagedCrypto.CreateDecryptor(this.Key, initializationVector), CryptoStreamMode.Read))
                        {
                            using (StreamReader sreader = new StreamReader(cryptoStream))
                            {
                                return sreader.ReadToEnd();
                            }
                        }

                    case 123:
                        throw new NotImplementedException($"Cannot decrypt DB of version: {fileVersion}\nLooks like you tried to decrypt a non-encrypted db.");
                    default:
                        throw new NotImplementedException($"Cannot decrypt DB of version: {fileVersion}");
                }
            }
        }

        /// <summary>
        /// Takes this database and stores it in the Filename as encrypted data
        /// </summary>
        private void EncryptFile()
        {
            RijndaelManaged rindaelManagedCrypto = new RijndaelManaged();

            byte[] initializationVector = new byte[this.Key.Length];

            var r = new Random();
            r.NextBytes(initializationVector); // fill the Initilization Vector with random Bytes
            Debug.Assert(initializationVector.Length == this.Key.Length && this.Key.Length == 16, "Encryption algorithm uses both a 16 byte key and initialization vector");

            var encryptor = rindaelManagedCrypto.CreateEncryptor(this.Key, initializationVector);

            if (!System.IO.File.Exists(this.Filename))
            {
                File.Create(this.Filename).Close();
            }

            // overwrite old file
            using (System.IO.FileStream fileStream = new System.IO.FileStream(this.Filename, FileMode.Truncate))
            {
                // write file version
                fileStream.WriteByte((byte)(this.DBVersion * 10));

                // write Initilization Vector
                fileStream.Write(initializationVector, 0, initializationVector.Length);
            }

            // re-open append mode, cryptStream needs a new fileStream
            using (System.IO.FileStream fileStream = new System.IO.FileStream(this.Filename, FileMode.Append))
            {
                using (CryptoStream cryptStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write))
                {
                    // Create a StreamWriter for easy writing to the filestream
                    using (StreamWriter streamWriter = new StreamWriter(cryptStream))
                    {
                        streamWriter.WriteLine(this.SerializeData);
                    }
                }
            }
        }
        #endregion
    }
}
