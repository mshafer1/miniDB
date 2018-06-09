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
        internal byte[] Key { get { return HardwareID; } }
        public EncryptedDataBase(string filename) : base(filename)
        {
            // NOOP
        }

        private EncryptedDataBase(string filename, bool base_case) : base(filename, base_case)
        {
            // NOOP
        }

        protected override DataBase<DBTransaction<T>> getTransactionsDB(string transactions_filename)
        {
            return new EncryptedDataBase<DBTransaction<T>>(transactions_filename, true);
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
            lock (_locker)
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
            byte[] IV = new byte[Key.Length];
            using (System.IO.FileStream fileStream = new FileStream(this.Filename, FileMode.Open))
            {
                var fileVersion = (short)fileStream.ReadByte();
                switch(fileVersion)
                {
                    case (12):
                    case (11):
                    case (10):
                        fileStream.Read(IV, 0, IV.Length);
                        fileStream.Seek(IV.Length + 1, SeekOrigin.Begin); // + 1 for version byte
                        using (CryptoStream cryptoStream = new CryptoStream(fileStream, RMCrypto.CreateDecryptor(Key, IV), CryptoStreamMode.Read))
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

            byte[] IV = new byte[Key.Length];

            var r = new Random();
            r.NextBytes(IV); // fill the IV with random Bytes
            Debug.Assert(IV.Length == Key.Length && Key.Length == 16);

            var encryptor = RMCrypto.CreateEncryptor(Key, IV);

            if(!System.IO.File.Exists(this.Filename))
            {
                File.Create(this.Filename).Close();
            }

            using (System.IO.FileStream fileStream = new System.IO.FileStream(this.Filename, FileMode.Truncate))
            {
                // write file version
                fileStream.WriteByte((int)(db_version * 10));

                // write IV
                fileStream.Write(IV, 0, IV.Length);
            }

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

        private static byte[] HardwareID { get { return DBHardwareID.IDValueBytes().Take(16).ToArray(); } }
        //private static ulong HardwardIDLow { get { } }
        
    }
}
