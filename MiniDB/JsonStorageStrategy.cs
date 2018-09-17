using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    static class JsonStorageStrategy<T> : IStorageStrategy<T> where T : DatabaseObject
    {
        public static void _cacheDB(DataBase<T> db)
        {
            var json = this.SerializeData(db);
            System.IO.File.WriteAllText(db.Filename, json);
        }

        public static DataBase<DBTransaction<T>> _getTransactionsDB(string transactions_filename)
        {
            throw new NotImplementedException();
        }

        public static DataBase<T> _loadDB(string filename)
        {
            var json = this._readFile(filename);
            if(json.Length > 0)
            {
                return JsonConvert.DeserializeObject<DataBase<T>>(json, new DataBaseSerializer<T>());
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Read in the file
        /// </summary>
        /// <param name="filename">the file (relative name or absolute path)</param>
        /// <returns>file contents</returns>
        private static string _readFile(string filename)
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
        private static string SerializeData(DataBase<T> db)
        {
            // TODO: compress https://dotnet-snippets.de/snippet/strings-komprimieren-und-dekomprimieren/1058
            return JsonConvert.SerializeObject(db, new DataBaseSerializer<T>());
        }

        public static void _migrate(string filename, float oldVersion, float newVersion)
        {
            throw new NotImplementedException();
        }
    }
}
