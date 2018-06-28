using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    /// <summary>
    /// An extension to PropertyChanged that includes the old and new values in addition to the changed properties
    /// from https://stackoverflow.com/a/7742890 modified
    /// </summary>
    public class PropertyChangedExtendedEventArgs : PropertyChangedEventArgs
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangedExtendedEventArgs"/> class.
        /// </summary>
        /// <param name="propertyName">The changed property</param>
        /// <param name="oldValue">The old value</param>
        /// <param name="newValue">The new value</param>
        /// <param name="undoable">Whether this change is undoable or not</param>
        public PropertyChangedExtendedEventArgs(string propertyName, object oldValue, object newValue, bool undoable = true)
            : base(propertyName)
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
            this.UndoableChange = undoable;
        }
        #endregion

        #region properties
        /// <summary>
        /// Gets the old value of the changed property
        /// </summary>
        public object OldValue { get; }

        /// <summary>
        /// Gets the new value of the changed property
        /// </summary>
        public object NewValue { get; }

        /// <summary>
        /// Gets a value indicating whether this change is undoable or not
        /// </summary>
        public bool UndoableChange { get; }
        #endregion
    }
}
