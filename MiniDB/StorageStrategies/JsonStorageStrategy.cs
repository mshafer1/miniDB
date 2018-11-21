using MiniDB.Transactions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    public class JsonStorageStrategy<T> : IStorageStrategy where T : IDatabaseObject
    {
        private readonly float DBVersion;
        private readonly float MinimumCompatibleVersion;

        public JsonStorageStrategy(float dbVersion, float minimumCompatibleVersion)
        {
            DBVersion = dbVersion;
            MinimumCompatibleVersion = minimumCompatibleVersion;
        }

        public void _cacheTransactions(ObservableCollection<IDBTransaction> dBTransactions, string transactionsFilename)
        {
            var json = JsonConvert.SerializeObject(dBTransactions);
            System.IO.File.WriteAllText(transactionsFilename, json);
        }

        public void _cacheDB(DataBase db)
        {
            var json = this.serializeDB(db);
            System.IO.File.WriteAllText(db.Filename, json);
        }

        public ObservableCollection<IDBTransaction> _getTransactionsCollection(string filename)
        {
            /*this.DBVersion = databaseVersion;
            this.Filename = filename;
            this.LoadFile(filename, false);*/
            var json = _readFile(filename);
            if (json.Length == 0)
            {
                return new ObservableCollection<IDBTransaction>();
            }

            var adapted = JsonConvert.DeserializeObject<ObservableCollection<DBTransactionInfo>>(json);
            var result = new ObservableCollection<IDBTransaction>();

            foreach(var item in adapted)
            {
                result.Add(DBTransactionInfo.GetDBTransaction(item));
            }

            return result;
        }

        public DataBase _loadDB(string filename)
        {
            var json = _readFile(filename);
            var result = new DataBase(filename, this.DBVersion, this.MinimumCompatibleVersion);
            if (json.Length == 0)
            {
                return result;
            }

            var adapted = JsonConvert.DeserializeObject<DataBase>(json, new DataBaseSerializer<T>());
            if (adapted.DBVersion > this.DBVersion)
            {
                throw new DBCreationException($"Cannot load db of version {adapted.DBVersion}. Current version is only {this.DBVersion}");
            }

            // TODO: implement migration callback

            // if still not new enough or too new
            if (adapted.DBVersion < this.MinimumCompatibleVersion || adapted.DBVersion > this.DBVersion)
            {
                throw new DBCreationException($"Cannot load db of version {adapted.DBVersion}. Current version is only {this.DBVersion} and only supports back to {this.MinimumCompatibleVersion}");
            }

            
            // parse and load
            foreach (var item in adapted)
            {
                result.Add(item);
            }

            return result;
        }

        public void _migrate(float oldVersion, float newVersion)
        {
            throw new NotImplementedException();
        }

        private string _readFile(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                var json = System.IO.File.ReadAllText(filename);
                return json;
            }

            return string.Empty;
        }

        private string serializeDB(DataBase db)
        {
            // TODO: compress https://dotnet-snippets.de/snippet/strings-komprimieren-und-dekomprimieren/1058
            return JsonConvert.SerializeObject(db, new DataBaseSerializer<T>());
        }
    }
}
