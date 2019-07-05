using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using MiniDB.Interfaces;
using MiniDB.Transactions;
using Newtonsoft.Json;

namespace MiniDB
{
    public class EncryptedStorageStrategy<T> : IStorageStrategy
        where T : IDBObject
    {
        #region Fields
        private readonly float dBVersion;
        private readonly float minimumCompatibleVersion;
        #endregion

        #region Constructors
        public EncryptedStorageStrategy(float dbVersion, float minimumCompatibleVersion)
        {
            this.dBVersion = dbVersion;
            this.minimumCompatibleVersion = minimumCompatibleVersion;
        }
        #endregion

        #region Properties

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
        /// Gets the version number to track how data is encrypted (stored only to one decimal place in one byte so max value of 25.5, and 12.3 is reserved for non-encrypted db's)
        /// </summary>
        protected virtual float EncryptionVersion { get; } = 1.0f;

        /// <summary>
        /// Gets a 16 byte hardware specific ID - used in encrypting/decrypting the database
        /// </summary>
        private static byte[] HardwareID
        {
            get => DBHardwareID.IDValueBytes().Take(16).ToArray();
        }

        #endregion

        public void CacheTransactions(ObservableCollection<IDBTransaction> dBTransactions, string filename)
        {
            this.EncryptFile(filename, JsonConvert.SerializeObject(dBTransactions));
        }

        public void CacheDB(DataBase db)
        {
            this.EncryptFile(db.Filename, JsonStorageStrategy<T>.SerializeDB(db));
        }

        public ObservableCollection<IDBTransaction> GetTransactionsCollection(string filename)
        {
            var json = this.ReadFile(filename);
            if (json.Length == 0)
            {
                return new ObservableCollection<IDBTransaction>();
            }

            var adapted = JsonConvert.DeserializeObject<ObservableCollection<DBTransactionInfo>>(json);
            var result = new ObservableCollection<IDBTransaction>();

            foreach (var item in adapted)
            {
                result.Add(DBTransactionInfo.GetDBTransaction(item));
            }

            return result;
        }

        public DBMetadata LoadDB(string filename)
        {
            var json = this.ReadFile(filename);
            var result = new DBMetadata(filename, this.dBVersion, this.minimumCompatibleVersion);
            if (json.Length == 0)
            {
                return result;
            }

            var adapted = JsonConvert.DeserializeObject<DataBase>(json, new DataBaseSerializer<T>());
            if (adapted.DBVersion > this.dBVersion)
            {
                throw new DBCreationException($"Cannot load db of version {adapted.DBVersion}. Current version is only {this.dBVersion}");
            }

            // TODO: implement migration callback

            // if still not new enough or too new
            if (adapted.DBVersion < this.minimumCompatibleVersion || adapted.DBVersion > this.dBVersion)
            {
                throw new DBCreationException($"Cannot load db of version {adapted.DBVersion}. Current version is only {this.dBVersion} and only supports back to {this.minimumCompatibleVersion}");
            }

            // parse and load
            foreach (var item in adapted)
            {
                result.Add(item);
            }

            return result;
        }

        public void Migrate(float oldVersion, float newVersion)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Overides the default ReadFile to decrypt the file on load
        /// </summary>
        /// <param name="filename">The filename or path to read</param>
        /// <returns>Json searlized <see cref="DataBase{T}" /> from file.</returns>
        protected string ReadFile(string filename)
        {
            if (File.Exists(filename))
            {
                var json = this.DecryptFile(filename);
                return json;
            }

            return string.Empty;
        }

        #region helper methods

        /// <summary>
        /// Helper method to load in the file, decrypt the binary blob, and return valid data - if overriding this class, will need to extend this method to handle custom versions
        /// </summary>
        /// <param name="filename">the file or path to load</param>
        /// <returns>DB JSON representation from binary blob in file</returns>
        protected virtual string DecryptFile(string filename)
        {
            // Create a new instance of the RijndaelManaged class
            //  and decrypt the stream.
            RijndaelManaged rindaelManagedCrypto = new RijndaelManaged();
            byte[] initializationVector = new byte[this.Key.Length];
            using (FileStream fileStream = new FileStream(filename, FileMode.Open))
            {
                var fileVersion = (short)fileStream.ReadByte();

                // TODO: implement overidable call back for migrating encryption versions in this db.
                switch (fileVersion)
                {
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
        /// Takes this database and stores it in the filename as encrypted data
        /// </summary>
        private void EncryptFile(string filename, string data)
        {
            RijndaelManaged rindaelManagedCrypto = new RijndaelManaged();

            byte[] initializationVector = new byte[this.Key.Length];

            var r = new Random();
            r.NextBytes(initializationVector); // fill the Initilization Vector with random Bytes
            Debug.Assert(initializationVector.Length == this.Key.Length && this.Key.Length == 16, "Encryption algorithm uses both a 16 byte key and initialization vector");

            var encryptor = rindaelManagedCrypto.CreateEncryptor(this.Key, initializationVector);

            if (!File.Exists(filename))
            {
                File.Create(filename).Close();
            }

            // overwrite old file
            using (FileStream fileStream = new FileStream(filename, FileMode.Truncate))
            {
                // write file version
                fileStream.WriteByte((byte)(this.EncryptionVersion * 10));

                // write Initilization Vector
                fileStream.Write(initializationVector, 0, initializationVector.Length);
            }

            // re-open append mode, cryptStream needs a new fileStream
            using (FileStream fileStream = new FileStream(filename, FileMode.Append))
            {
                using (CryptoStream cryptStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write))
                {
                    // Create a StreamWriter for easy writing to the filestream
                    using (StreamWriter streamWriter = new StreamWriter(cryptStream))
                    {
                        streamWriter.WriteLine(data);
                    }
                }
            }
        }
        #endregion
    }
}
