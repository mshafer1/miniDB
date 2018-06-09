using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MiniDB
{
    public abstract class BaseModelItem : INotifyPropertyChangedExtended
    {
        protected BaseModelItem()
        {
            fields = new Dictionary<string, object>();
        }

        public event PropertyChangedExtendedEventHandler PropertyChangedExtended;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChangedExtended(object oldValue, object newValue, string propertyName, bool undoable = true)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedExtendedEventArgs(propertyName, oldValue, newValue));
            PropertyChangedExtended?.Invoke(this, new PropertyChangedExtendedEventArgs(propertyName, oldValue, newValue, undoable));
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
                if (oldVal == null && value == null)
                {
                    return false;
                }
                if (oldVal != null && oldVal.Equals(value))
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
                this.OnPropertyChangedExtended(oldVal, value, name, undoable);
            }
            return true;
        }

        protected Dictionary<string, object> fields;
    }
}
