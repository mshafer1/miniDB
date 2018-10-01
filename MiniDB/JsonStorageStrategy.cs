using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    public class JsonStorageStrategy<T> : IStorageStrategy<T> where T : IDatabaseObject
    {
        //private readonly string dbFile;
        //private readonly string transaction_filename;
        //private readonly float dbVersion;
        //// TODO implement migration;

        //public JsonStorageStrategy(string filename, float dbVersion, object migrationTool = null)
        //{
        //    this.dbFile = filename;
        //    this.transaction_filename = $"_transaction_{filename}";

        //    // TODO, store migration tool
        //}

        //public void cacheTransactions(ObservableCollection<DBTransaction<T>> dBTransactions)
        //{
        //    throw new NotImplementedException();
        //}

        //public void _cacheDB(DataBase<T> db)
        //{
        //    throw new NotImplementedException();
        //}

        //public ObservableCollection<DBTransaction<T>> _getTransactionsCollection()
        //{
        //    var json = this._readFile(this.transaction_filename);
        //    ObservableCollection<DBTransaction<T>> result;
        //    if (json.Length > 0)
        //    {
        //        result = JsonConvert.DeserializeObject<ObservableCollection<DBTransaction<T>>>(json, new DataBaseSerializer<T>());
        //        // TODO: migration call back
        //    }
        //    else
        //    {
        //        result = new ObservableCollection<DBTransaction<T>>();
        //    }
        //    return result;
        //}

        //public DataBase<T> _loadDB()
        //{
        //    throw new NotImplementedException();
        //}

        //public void _migrate(float oldVersion, float newVersion)
        //{
        //    throw new NotImplementedException();
        //}

        ///// <summary>
        ///// Read in the file
        ///// </summary>
        ///// <param name="filename">the file (relative name or absolute path)</param>
        ///// <returns>file contents</returns>
        //private string _readFile(string filename)
        //{
        //    if (System.IO.File.Exists(filename))
        //    {
        //        var json = System.IO.File.ReadAllText(filename);
        //        return json;
        //    }

        //    return string.Empty;
        //}

        ///// <summary>
        ///// Gets Json serialized value of this as string
        ///// </summary>
        //internal static string SerializeData(DataBase<T> db)
        //{
        //    // TODO: compress https://dotnet-snippets.de/snippet/strings-komprimieren-und-dekomprimieren/1058
        //    return JsonConvert.SerializeObject(db, new DataBaseSerializer<T>());
        //}
        public void cacheTransactions(ObservableCollection<DBTransaction<T>> dBTransactions)
        {
            throw new NotImplementedException();
        }

        public void _cacheDB<T2>(DataBase<T2> db) where T2 : DatabaseObject
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<DBTransaction<T>> _getTransactionsCollection()
        {
            throw new NotImplementedException();
        }

        public DataBase<T2> _loadDB<T2>() where T2 : DatabaseObject
        {
            throw new NotImplementedException();
        }

        public void _migrate(float oldVersion, float newVersion)
        {
            throw new NotImplementedException();
        }
    }
}
