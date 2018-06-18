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
        public object OldValue { get; }
        public object NewValue { get; }
        public bool UndoableChange { get; }

        public PropertyChangedExtendedEventArgs(string propertyName, object oldValue, object newValue, bool undoable=true)
            : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
            UndoableChange = undoable;
        }
    }
}
