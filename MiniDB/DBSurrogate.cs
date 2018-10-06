using System.Collections.ObjectModel;

namespace MiniDB
{
    /// <summary>
    /// intermediate class that can be serialized by JSON.net
    /// </summary>
    /// <typeparam name="T">and contains the same data as <see cref="DataBase{T}"/> - allows for DBVersion to be serialized</typeparam>
    internal class DBSurrogate<T> where T : IDatabaseObject
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
}
