using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    class JsonStorageStrategy<T> : IStorageStrategy<T> where T : DatabaseObject
    {
        private static JsonStorageStrategy<T> instance = null;
        public static JsonStorageStrategy<T> Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new JsonStorageStrategy<T>();
                }
                return instance;
            }
        }

        private JsonStorageStrategy() { }

        public DataBase<DBTransaction<T>> _getTransactionsDB(string transactions_filename, float dbVersion, float minimumCompatibleVersion, IStorageStrategy<DatabaseObject> storageStrategy)
        {
            var json = this._readFile(transactions_filename);
            DataBase<DBTransaction<T>> result;
            if (json.Length > 0)
            {
                result = JsonConvert.DeserializeObject<DataBase<DBTransaction<T>>>(json, new DataBaseSerializer<T>());
                // TODO: migration call back
            }
            else
            {
                result = new DataBase<DBTransaction<T>>(transactions_filename, dbVersion, minimumCompatibleVersion, storageStrategy);
            }
            return result;
        }

        public DataBase<T> _loadDB(string filename)
        {
            var json = this._readFile(filename);
            if (json.Length > 0)
            {
                return JsonConvert.DeserializeObject<DataBase<T>>(json, new DataBaseSerializer<T>());
            }
            else
            {
                return null;
            }
        }

        public void _cacheDB(DataBase<T> db)
        {
            var json = this.SerializeData(db);
            System.IO.File.WriteAllText(db.Filename, json);
        }

        public void _migrate(string filename, float oldVersion, float newVersion)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Read in the file
        /// </summary>
        /// <param name="filename">the file (relative name or absolute path)</param>
        /// <returns>file contents</returns>
        private string _readFile(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                var json = System.IO.File.ReadAllText(filename);
                return json;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets Json serialized value of this as string
        /// </summary>
        internal string SerializeData(DataBase<T> db)
        {
            // TODO: compress https://dotnet-snippets.de/snippet/strings-komprimieren-und-dekomprimieren/1058
            return JsonConvert.SerializeObject(db, new DataBaseSerializer<T>());
        }
    }
}
