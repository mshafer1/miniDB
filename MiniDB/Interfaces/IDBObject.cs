using System.ComponentModel;

namespace MiniDB.Interfaces
{
    public interface IDBObject : INotifyPropertyChanged, INotifyPropertyChangedExtended
    {
        #region Fields

        ID ID { get; }

        #endregion
    }
}