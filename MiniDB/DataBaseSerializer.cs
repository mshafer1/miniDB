using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    //from https://stackoverflow.com/a/14384830/8100990
    // possible alternative https://stackoverflow.com/a/22486943/8100990
    
    // intermediate class that can be serialized by JSON.net
    // and contains the same data as Database<T> - allows for DBVersion to be serialized
    class DataBaseSurrogate<T> where T : DatabaseObject
    {
        // the collection of T elements
        public ObservableCollection<T> Collection { get; set; }
        // the properties of DataBase to serialize
        public float DBVersion { get; set; }
    }


    class DataBaseSerializer<T> : JsonConverter where T : DatabaseObject
    {

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DataBase<T>) || objectType.IsSubclassOf(typeof(DataBase<T>));
        }

        public override object ReadJson(
            JsonReader reader, Type objectType,
            object existingValue, JsonSerializer serializer)
        {
            // N.B. null handling is missing
            var surrogate = serializer.Deserialize<DataBaseSurrogate<T>>(reader);
            var elements = surrogate.Collection;
            var db = new DataBase<T>() { DBVersion = surrogate.DBVersion };
            foreach (var el in elements)
                db.Add(el);
            return db;
        }

        public override void WriteJson(JsonWriter writer, object value,
                                       JsonSerializer serializer)
        {
            // N.B. null handling is missing
            var db = (DataBase<T>)value;
            // create the surrogate and serialize it instead 
            // of the collection itself
            var surrogate = new DataBaseSurrogate<T>()
            {
                Collection = new ObservableCollection<T>(db),
                DBVersion = db.DBVersion
            };

            // from https://stackoverflow.com/questions/7397207/json-net-error-self-referencing-loop-detected-for-type
            serializer.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
            serializer.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            serializer.Serialize(writer, surrogate);
        }
    }

}
