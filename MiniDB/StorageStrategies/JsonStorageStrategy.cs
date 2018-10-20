using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    public class JsonStorageStrategy : IStorageStrategy
    {
        private readonly float DBVersion;
        private readonly float MinimumCompatibleVersion;

        public JsonStorageStrategy(float dbVersion, float minimumCompatibleVersion)
        {
            DBVersion = dbVersion;
            MinimumCompatibleVersion = minimumCompatibleVersion;
        }

        public void cacheTransactions(ObservableCollection<DBTransaction> dBTransactions)
        {
            throw new NotImplementedException();
        }

        public void _cacheDB(DataBase db)
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<DBTransaction> _getTransactionsCollection(string filename)
        {
            /*this.DBVersion = databaseVersion;
            this.Filename = filename;
            this.LoadFile(filename, false);*/
            var json = _readFile(filename);
            if (json.Length == 0)
            {
                return new ObservableCollection<DBTransaction>();
            }

            var adapted = JsonConvert.DeserializeObject<ObservableCollection<DBTransaction>>(json);
            return adapted;
        }

        public DataBase _loadDB(string filename)
        {
            var json = _readFile(filename);
            var result = new DataBase(filename, this.DBVersion, this.MinimumCompatibleVersion);
            if (json.Length == 0)
            {
                return result;
            }

            var adapted = JsonConvert.DeserializeObject<DataBase>(json, new DataBaseSerializer());
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
    }
}
