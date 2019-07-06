using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using MiniDB.Transactions;

namespace MiniDB.Interfaces
{
    public interface IUndoRedoManager
    {
        bool CheckCanUndo();

        bool CheckCanRedo();

        void Undo(IList<IDBObject> dataToActOn, NotifyCollectionChangedEventHandler dataChangedHandler, PropertyChangedExtendedEventHandler propertyChangedHandler);

        void Redo(Collection<IDBObject> dataToActOn, NotifyCollectionChangedEventHandler dataChangedHandler, PropertyChangedExtendedEventHandler propertyChangedHandler);

        void InsertTransaction(IDBTransaction transaction);
    }
}
