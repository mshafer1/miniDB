using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Security.Cryptography;
using System.IO;
using System.Diagnostics;

namespace MiniDB
{
    public class EncryptedDataBase<T> : DataBase<T> where T : DatabaseObject
    {
        /// <summary>
        /// The private key used in the encryption/decryption
        /// </summary>
        internal byte[] Key { get { return HardwareID; } }
        public EncryptedDataBase(string filename, float DBVersion, float MinimumCompatibleVersion) : base(filename, DBVersion, MinimumCompatibleVersion)
        {
            // NOOP
        }

        private EncryptedDataBase(string filename, float DBVersion, float MinimumCompatibleVersion, bool base_case) : base(filename, DBVersion, MinimumCompatibleVersion, base_case)
        {
            // NOOP
        }

        protected override DataBase<DBTransaction<T>> getTransactionsDB(string transactions_filename)
        {
            return new EncryptedDataBase<DBTransaction<T>>(transactions_filename, this.DBVersion, this.MinimumCompatibleVersion, true);
        }

        protected override string readFile(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                var json = decryptFile(filename);
                return json;
            }
            return "";
        }

        protected override void _cacheDB()
        {
            lock (Locker)
            {
                // store local version
                encryptFile();
            }
        }

        private string decryptFile(string filename)
        {
            //Create a new instance of the RijndaelManaged class  
            // and decrypt the stream.  
            RijndaelManaged RMCrypto = new RijndaelManaged();
            byte[] InitializationVector = new byte[Key.Length];
            using (System.IO.FileStream fileStream = new FileStream(this.Filename, FileMode.Open))
            {
                var fileVersion = (short)fileStream.ReadByte();
                switch (fileVersion)
                {
                    case (12):
                    case (11):
                    case (10):
                        fileStream.Read(InitializationVector, 0, InitializationVector.Length);
                        fileStream.Seek(InitializationVector.Length + 1, SeekOrigin.Begin); // + 1 for version byte
                        using (CryptoStream cryptoStream = new CryptoStream(fileStream, RMCrypto.CreateDecryptor(Key, InitializationVector), CryptoStreamMode.Read))
                        {
                            using (StreamReader sreader = new StreamReader(cryptoStream))
                            {
                                //sreader.ReadToEnd(); 
                                return sreader.ReadToEnd();
                            }
                        }
                    case (123):
                        throw new NotImplementedException($"Cannot decrypt DB of version: {fileVersion}\nLooks like you tried to decrypt a non-encrypted db.");
                    default:
                        throw new NotImplementedException($"Cannot decrypt DB of version: {fileVersion}");
                }
            }
        }

        private void encryptFile()
        {
            RijndaelManaged RMCrypto = new RijndaelManaged();

            byte[] InitializationVector = new byte[Key.Length];

            var r = new Random();
            r.NextBytes(InitializationVector); // fill the Initilization Vector with random Bytes
            Debug.Assert(InitializationVector.Length == Key.Length && Key.Length == 16);

            var encryptor = RMCrypto.CreateEncryptor(Key, InitializationVector);

            if (!System.IO.File.Exists(this.Filename))
            {
                File.Create(this.Filename).Close();
            }

            // overwrite old file
            using (System.IO.FileStream fileStream = new System.IO.FileStream(this.Filename, FileMode.Truncate))
            {
                // write file version
                fileStream.WriteByte((byte)(base.DBVersion * 10));

                // write Initilization Vector
                fileStream.Write(InitializationVector, 0, InitializationVector.Length);
            }

            // re-open append mode, cryptStream needs a new fileStream
            using (System.IO.FileStream fileStream = new System.IO.FileStream(this.Filename, FileMode.Append))
            {
                using (CryptoStream CryptStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write))
                {
                    //Create a StreamWriter for easy writing to the filestream
                    using (StreamWriter SWriter = new StreamWriter(CryptStream))
                    {
                        SWriter.WriteLine(serializeData);
                    }
                }
            }
        }

        /// <summary>
        /// 16 byte hardware specific ID - used in encrypting/decrypting the database
        /// </summary>
        private static byte[] HardwareID { get { return DBHardwareID.IDValueBytes().Take(16).ToArray(); } }
    }
}
