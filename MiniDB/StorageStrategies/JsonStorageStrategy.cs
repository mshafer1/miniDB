using System;
using System.Collections.ObjectModel;

using MiniDB.Interfaces;
using MiniDB.Transactions;
using Newtonsoft.Json;

namespace MiniDB
{
    public class JsonStorageStrategy<T> : IStorageStrategy
        where T : IDBObject
    {
        private readonly float dBVersion;
        private readonly float minimumCompatibleVersion;

        public JsonStorageStrategy(float dbVersion, float minimumCompatibleVersion)
        {
            this.dBVersion = dbVersion;
            this.minimumCompatibleVersion = minimumCompatibleVersion;
        }

        public static string SerializeDB(DataBase db)
        {
            // TODO: compress https://dotnet-snippets.de/snippet/strings-komprimieren-und-dekomprimieren/1058
            return JsonConvert.SerializeObject(db, new DataBaseSerializer<T>());
        }

        void ITransactionStorageStrategy.CacheTransactions(ObservableCollection<IDBTransaction> dBTransactions, string transactionsFilename)
        {
            var json = JsonConvert.SerializeObject(dBTransactions);
            System.IO.File.WriteAllText(transactionsFilename, json);
        }

        public void CacheDB(DataBase db)
        {
            var json = SerializeDB(db);
            System.IO.File.WriteAllText(db.Filename, json);
        }

        ObservableCollection<IDBTransaction> ITransactionStorageStrategy.GetTransactionsCollection(string filename)
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

        private string ReadFile(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                var json = System.IO.File.ReadAllText(filename);
                return json;
            }

            return string.Empty;
        }
    }
}
