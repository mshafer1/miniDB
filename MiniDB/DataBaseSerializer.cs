﻿using System;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace MiniDB
{
    // from https://stackoverflow.com/a/14384830/8100990
    //   possible alternative https://stackoverflow.com/a/22486943/8100990

    /// <summary>
    /// intermediate class that can be serialized by JSON.net
    /// </summary>
    /// <typeparam name="T">and contains the same data as <see cref="DataBase{T}"/> - allows for DBVersion to be serialized</typeparam>
    internal class DataBaseSurrogate<T> where T : IDatabaseObject
    {
        // the collection of T elements

        /// <summary>
        /// Gets or sets the collection to cache
        /// Represent the Database data as an ObservableCollection of T (Database base class)
        /// </summary>
        public ObservableCollection<T> Collection { get; set; }
        
        // the properties of DataBase to serialize

        /// <summary>
        /// Gets or sets the DBVersion
        /// Make sure DBVersion gets included in serialization
        /// </summary>
        public float DBVersion { get; set; }
    }

    /// <summary>
    /// Class to serialize database to json with db version number
    /// </summary>
    /// <typeparam name="T">The type of object stored in the Database</typeparam>
    internal class DataBaseSerializer<T> : JsonConverter where T : IDatabaseObject
    {
        /// <summary>
        /// Verify if JsonConverter can be used to convert this object from Json
        /// </summary>
        /// <param name="objectType">The type of the object that is in question</param>
        /// <returns>True if able to case object to Database of T</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DataBase<T>) || objectType.IsSubclassOf(typeof(DataBase<T>));
        }

        /// <summary>
        /// Use the reader and desired type to load a Database
        /// </summary>
        /// <param name="reader">the reader to use</param>
        /// <param name="objectType">type of object</param>
        /// <param name="existingValue">existing value</param>
        /// <param name="serializer">serializer to use</param>
        /// <returns>Database object</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // N.B. null handling is missing
            var surrogate = serializer.Deserialize<DataBaseSurrogate<T>>(reader);
            var elements = surrogate.Collection;
            var db = new DataBase<T>() { DBVersion = surrogate.DBVersion };
            foreach (var el in elements)
            {
                db.Add(el);
            }

            return db;
        }

        /// <summary>
        /// Write a serialization of the DB to the writer that is passed in
        /// </summary>
        /// <param name="writer">JsonWriter the writer to shove the json in</param>
        /// <param name="value">the DataBase to write to json</param>
        /// <param name="serializer">JsonSerializer to </param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
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
