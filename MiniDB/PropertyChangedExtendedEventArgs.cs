using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    //from https://stackoverflow.com/a/7742890 modified
    public class PropertyChangedExtendedEventArgs : PropertyChangedEventArgs
    {
        public virtual object OldValue { get; }
        public virtual object NewValue { get; }
        public virtual bool undoableChange { get; }

        public PropertyChangedExtendedEventArgs(string propertyName, object oldValue, object newValue, bool undoable=true)
            : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
            undoableChange = undoable;
        }
    }
}
