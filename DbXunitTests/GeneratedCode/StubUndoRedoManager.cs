using System;
using System.Collections.Generic;

// this file was created by AutoImplement
namespace MiniDB.Interfaces
{
    public class StubUndoRedoManager : IUndoRedoManager
    {
        public Func<bool> CheckCanUndo { get; set; }

        public Func<bool> CheckCanRedo { get; set; }

        public Action<IList<IDBObject>, System.Collections.Specialized.NotifyCollectionChangedEventHandler, PropertyChangedExtendedEventHandler> Undo { get; set; }

        public Action<System.Collections.ObjectModel.Collection<IDBObject>, System.Collections.Specialized.NotifyCollectionChangedEventHandler, PropertyChangedExtendedEventHandler> Redo { get; set; }

        public Action<Transactions.IDBTransaction> InsertTransaction { get; set; }

        bool IUndoRedoManager.CheckCanUndo()
        {
            if (this.CheckCanUndo != null)
            {
                return this.CheckCanUndo();
            }
            else
            {
                return default(bool);
            }
        }

        void IUndoRedoManager.Redo(System.Collections.ObjectModel.Collection<IDBObject> dataToActOn, System.Collections.Specialized.NotifyCollectionChangedEventHandler dataChangedHandler, PropertyChangedExtendedEventHandler propertyChangedHandler)
        {
            this.Redo?.Invoke(dataToActOn, dataChangedHandler, propertyChangedHandler);
        }

        bool IUndoRedoManager.CheckCanRedo()
        {
            if (this.CheckCanRedo != null)
            {
                return this.CheckCanRedo();
            }
            else
            {
                return default(bool);
            }
        }

        void IUndoRedoManager.Undo(IList<IDBObject> dataToActOn, System.Collections.Specialized.NotifyCollectionChangedEventHandler dataChangedHandler, PropertyChangedExtendedEventHandler propertyChangedHandler)
        {
            this.Undo?.Invoke(dataToActOn, dataChangedHandler, propertyChangedHandler);
        }

        void IUndoRedoManager.InsertTransaction(Transactions.IDBTransaction transaction)
        {
            this.InsertTransaction?.Invoke(transaction);
        }
    }
}
