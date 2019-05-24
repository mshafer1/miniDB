using System.Collections.ObjectModel;

using MiniDB.Interfaces;

namespace MiniDB
{
    /// <summary>
    /// intermediate class that can be serialized by JSON.net
    /// </summary>
    /// <typeparam name="T">and contains the same data as <see cref="DataBase{T}"/> - allows for DBVersion to be serialized</typeparam>
    internal class DBSurrogate
    {
        /// <summary>
        /// Gets or sets the collection to cache
        /// Represent the Database data as an ObservableCollection of T (Database base class)
        /// </summary>
        public ObservableCollection<IDBObject> Collection { get; set; }

        // the properties of DataBase to serialize

        /// <summary>
        /// Gets or sets the DBVersion
        /// Make sure DBVersion gets included in serialization
        /// </summary>
        public float DBVersion { get; set; }
    }
}
