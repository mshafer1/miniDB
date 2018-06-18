using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace MiniDB
{
    public abstract class DatabaseItem
    {
        public event PropertyChangedExtendedEventHandler PropertyChangedExtended;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raise property changed event args - contains just the changed property's name
        /// </summary>
        /// <param name="propertyName">Name of the property changed</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raise property changed extended - contains changed property name, the old value, the new value, and whether or not it should be undoable
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        /// <param name="undoable"></param>
        protected void OnPropertyChangedExtended(string propertyName, object oldValue, object newValue, bool undoable = true)
        {
            var args = new PropertyChangedExtendedEventArgs(propertyName, oldValue, newValue, undoable);
            PropertyChanged?.Invoke(this, args);
            PropertyChangedExtended?.Invoke(this, args);
        }

        /// <summary>
        /// Return the requested item by name from fields if it is there, else null.
        /// </summary>
        /// <param name="name">The name of the item to fetch (default: caller)</param>
        /// <returns></returns>
        protected dynamic Get([CallerMemberName]string name = null)
        { // TODO changing this to dynamic may make structs not work correctly unless initialized 
            return fields.ContainsKey(name) ? fields[name] : null;
        }

        /// <summary>
        /// Store the value in fields and raise a PropertyChangedExtended event
        ///   if the new value is different, else return false.
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="value">The desired value to store in fields</param>
        /// <param name="name">The name to store the value under (default: caller)</param>
        /// <returns></returns>
        protected bool Set<T>(T value, [CallerMemberName]string name = null, bool raiseEvent = true, bool undoable = true)
        {
            T oldVal;
            if (fields.ContainsKey(name))
            {
                oldVal = (T)fields[name];
                // if both old and new are null - or new value equals old value (handling possible null case)
                if ((value == null && oldVal == null) || (oldVal?.Equals(value) ?? false))
                {
                    return false; // NO-OP
                }
                fields[name] = value;
            }
            else
            {
                oldVal = default(T);
                fields.Add(name, value);
            }
            if (raiseEvent)
            {
                this.OnPropertyChangedExtended(name, oldVal, value, undoable);
            }
            return true;
        }

        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();
    }
}
