using System.ComponentModel;

namespace MiniDB
{
    public interface IDatabaseObject
    {
        #region events
        /// <summary>
        /// Event raised when specific undoable/settable properties are changed (includes how changed).
        /// </summary>
        event PropertyChangedExtendedEventHandler PropertyChangedExtended;

        /// <summary>
        /// Event raised when any property is changed (does not contain how changed).
        /// </summary>
        event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Fields

        ID ID { get; }

        #endregion
    }
}