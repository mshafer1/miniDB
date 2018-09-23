using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    class EncryptedStorageStrategy<T> : IStorageStrategy<T> where T : IDatabaseObject
    {
        private static IStorageStrategy<T> instance = null;
        public static IStorageStrategy<T> Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EncryptedStorageStrategy<T>();
                }
                return instance;
            }
        }

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

        private EncryptedStorageStrategy() { }


        public void _cacheDB(DataBase<T> db)
        {
            this.EncryptFile(db);
        }

        //public DataBase<DBTransaction<IDatabaseObject>> _getTransactionsDB(string transactions_filename)
        //{
        //    var json = this.DecryptFile(transactions_filename);
        //    if (json.Length > 0)
        //    {
        //        return JsonConvert.DeserializeObject<DataBase<DBTransaction<T>>>(json, new DataBaseSerializer<T>());
        //    }
        //    else
        //    {
        //        return new DataBase<DBTransaction<T>>();
        //    }
        //}

        public DataBase<T> _loadDB(string filename)
        {
            var json = this.DecryptFile(filename);
            if (json.Length > 0)
            {
                return JsonConvert.DeserializeObject<DataBase<T>>(json, new DataBaseSerializer<T>());
            }
            else
            {
                return new DataBase<T>();
            }
        }

        public void _migrate(string filename, float oldVersion, float newVersion)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Helper method to load in the file, decrypt the binary blob, and return valid data - if overriding this class, will need to extend this method to handle custom versions
        /// </summary>
        /// <param name="filename">the file or path to load</param>
        /// <returns>DB JSON representation from binary blob in file</returns>
        private string DecryptFile(string filename)
        {
            // Create a new instance of the RijndaelManaged class  
            //  and decrypt the stream.  
            RijndaelManaged rindaelManagedCrypto = new RijndaelManaged();
            byte[] initializationVector = new byte[this.Key.Length];
            using (System.IO.FileStream fileStream = new FileStream(filename, FileMode.Open))
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
        /// Takes this database and stores it in the Filename as encrypted data
        /// </summary>
        private void EncryptFile(DataBase<T> db)
        {
            RijndaelManaged rindaelManagedCrypto = new RijndaelManaged();

            byte[] initializationVector = new byte[this.Key.Length];

            var r = new Random();
            r.NextBytes(initializationVector); // fill the Initilization Vector with random Bytes
            Debug.Assert(initializationVector.Length == this.Key.Length && this.Key.Length == 16, "Encryption algorithm uses both a 16 byte key and initialization vector");

            var encryptor = rindaelManagedCrypto.CreateEncryptor(this.Key, initializationVector);

            if (!System.IO.File.Exists(db.Filename))
            {
                File.Create(db.Filename).Close();
            }

            // overwrite old file
            using (System.IO.FileStream fileStream = new System.IO.FileStream(db.Filename, FileMode.Truncate))
            {
                // write file version
                fileStream.WriteByte((byte)(this.EncryptionVersion * 10));

                // write Initilization Vector
                fileStream.Write(initializationVector, 0, initializationVector.Length);
            }

            // re-open append mode, cryptStream needs a new fileStream
            using (System.IO.FileStream fileStream = new System.IO.FileStream(db.Filename, FileMode.Append))
            {
                using (CryptoStream cryptStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write))
                {
                    // Create a StreamWriter for easy writing to the filestream
                    using (StreamWriter streamWriter = new StreamWriter(cryptStream))
                    {
                        streamWriter.Write(JsonStorageStrategy<T>.SerializeData(db));
                    }
                }
            }
        }

        DataBase<DBTransaction<IDatabaseObject>> IStorageStrategy<T>._getTransactionsDB(string transactions_filename, float dbVersion, float minimumCompatibleVersion, IStorageStrategy<DBTransaction<IDatabaseObject>> storageStrategy)
        {
            var json = this.DecryptFile(transactions_filename);
            if (json.Length > 0)
            {
                return JsonConvert.DeserializeObject<DataBase<DBTransaction<IDatabaseObject>>>(json, new DataBaseSerializer<T>());
            }
            else
            {
                return new DataBase<DBTransaction<IDatabaseObject>>();
            }
        }
    }
}
