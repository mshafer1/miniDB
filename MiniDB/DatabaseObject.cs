using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Newtonsoft.Json;

namespace MiniDB
{
    /// <summary>
    /// Base Database Object
    /// Provides: 
    ///   Get and Set methods that store corresponding property and raise the OnChangedExtendedEventArgs
    ///   PropertyChanged event (should be raised to update UI, but does not trigger a DB cache)
    ///   PropertyChangedExtended event (should be raised to update DB cache, also triggers PropertyChanged to update UI)
    /// </summary>
    public abstract class DatabaseObject
    {
        #region fields
        /// <summary>
        /// store the properties that are accessible via the Set and Get methods.
        /// </summary>
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();
        #endregion

        #region constructors
        /// <summary>
        /// /// Initializes a new instance of the <see cref="DatabaseObject" /> class.
        /// </summary>
        public DatabaseObject()
        {
            ID = new ID();
            ID.Set();
        }
        #endregion

        #region events
        /// <summary>
        /// Event raised when specific undoable/settable properties are changed (includes how changed).
        /// </summary>
        public event PropertyChangedExtendedEventHandler PropertyChangedExtended;

        /// <summary>
        /// Event raised when any property is changed (does not contain how changed).
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region properties
        /// <summary>
        /// Gets this dbObjects ID
        /// </summary>
        [JsonProperty]
        public ID ID { get; private set; } // using private set to prevent children classes from creating a new ID, but allowing Newtonsoft.json to tweak it.

        /// <summary>
        /// Re-assign the ID a new value.
        /// </summary>
        public void SetID() => ID.Set(); // randomly re-assign
        #endregion

        #region helper methods
        #region event raisers
        /// <summary>
        /// Raise property changed event args - contains just the changed property's name
        /// </summary>
        /// <param name="propertyName">Name of the property changed</param>
        protected void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raise property changed extended - contains changed property name, the old value, the new value, and whether or not it should be undoable
        /// </summary>
        /// <param name="propertyName">Name of the changed property</param>
        /// <param name="oldValue">The previous value</param>
        /// <param name="newValue">The new value</param>
        /// <param name="undoable">Whether this is an undoable action or not</param>
        protected void OnPropertyChangedExtended(string propertyName, object oldValue, object newValue, bool undoable = true)
        {
            var args = new PropertyChangedExtendedEventArgs(propertyName, oldValue, newValue, undoable);
            this.PropertyChanged?.Invoke(this, args);
            this.PropertyChangedExtended?.Invoke(this, args);
        }
        #endregion

        #region Accessors
        /// <summary>
        /// Return the requested item by name from fields if it is there, else null.
        /// </summary>
        /// <param name="name">The name of the item to fetch (default: caller)</param>
        /// <returns>The object value requested</returns>
        protected dynamic Get([CallerMemberName]string name = null)
        { // TODO changing this to dynamic may make structs not work correctly unless initialized 
            return this.fields.ContainsKey(name) ? this.fields[name] : null;
        }

        /// <summary>
        /// Store the value in fields and raise a PropertyChangedExtended event
        ///   if the new value is different, else return false.
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="value">The desired value to store in fields</param>
        /// <param name="name">The name to store the value under (default: caller)</param>
        /// <param name="raiseEvent">Specify whether or not to raise the OnPropertyChanged event</param>
        /// <param name="undoable">Specify if this is an undoablechange</param>
        /// <returns>False if no-op, else true</returns>
        protected bool Set<T>(T value, [CallerMemberName]string name = null, bool raiseEvent = true, bool undoable = true)
        {
            T oldVal;
            if (this.fields.ContainsKey(name))
            {
                oldVal = (T)this.fields[name];

                // if both old and new are null - or new value equals old value (handling possible null case)
                if ((value == null && oldVal == null) || (oldVal?.Equals(value) ?? false))
                {
                    return false; // NO-OP
                }

                this.fields[name] = value;
            }
            else
            {
                oldVal = default(T);
                this.fields.Add(name, value);
            }

            if (raiseEvent)
            {
                this.OnPropertyChangedExtended(name, oldVal, value, undoable);
            }

            return true;
        }
        #endregion
        #endregion
    }
}
