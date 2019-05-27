using System;
using System.Collections.Generic;
using System.Delegation;

// this file was created by AutoImplement
namespace MiniDB.Interfaces
{
    public class StubUndoRedoManager : IUndoRedoManager
    {
        public Func<bool> CheckCanUndo { get; set; }
        
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
        
        public Func<bool> CheckCanRedo { get; set; }
        
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
        
        public Action<System.Collections.Generic.IList<IDBObject>, System.Collections.Specialized.NotifyCollectionChangedEventHandler, MiniDB.PropertyChangedExtendedEventHandler> Undo { get; set; }
        
        void IUndoRedoManager.Undo(System.Collections.Generic.IList<IDBObject> dataToActOn, System.Collections.Specialized.NotifyCollectionChangedEventHandler dataChangedHandler, MiniDB.PropertyChangedExtendedEventHandler propertyChangedHandler)
        {
            if (this.Undo != null)
            {
                this.Undo(dataToActOn, dataChangedHandler, propertyChangedHandler);
            }
        }
        
        public Action<System.Collections.ObjectModel.Collection<IDBObject>, System.Collections.Specialized.NotifyCollectionChangedEventHandler, MiniDB.PropertyChangedExtendedEventHandler> Redo { get; set; }
        
        void IUndoRedoManager.Redo(System.Collections.ObjectModel.Collection<IDBObject> dataToActOn, System.Collections.Specialized.NotifyCollectionChangedEventHandler dataChangedHandler, MiniDB.PropertyChangedExtendedEventHandler propertyChangedHandler)
        {
            if (this.Redo != null)
            {
                this.Redo(dataToActOn, dataChangedHandler, propertyChangedHandler);
            }
        }
        
        public Action<MiniDB.Transactions.IDBTransaction> InsertTransaction { get; set; }
        
        void IUndoRedoManager.InsertTransaction(MiniDB.Transactions.IDBTransaction transaction)
        {
            if (this.InsertTransaction != null)
            {
                this.InsertTransaction(transaction);
            }
        }
        
    }
}
