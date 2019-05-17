using System;
using System.Collections.Generic;
using System.Delegation;

// this file was created by AutoImplement
namespace MiniDB.Interfaces
{
    public class StubUndoRedoManager : IUndoRedoManager
    {
        public Func<System.Collections.Generic.IEnumerable<MiniDB.Transactions.IDBTransaction>, bool> CheckCanUndo { get; set; }
        
        bool IUndoRedoManager.CheckCanUndo(System.Collections.Generic.IEnumerable<MiniDB.Transactions.IDBTransaction> transactions)
        {
            if (this.CheckCanUndo != null)
            {
                return this.CheckCanUndo(transactions);
            }
            else
            {
                return default(bool);
            }
        }
        
        public Func<System.Collections.Generic.IEnumerable<MiniDB.Transactions.IDBTransaction>, bool> CheckCanRedo { get; set; }
        
        bool IUndoRedoManager.CheckCanRedo(System.Collections.Generic.IEnumerable<MiniDB.Transactions.IDBTransaction> transactions)
        {
            if (this.CheckCanRedo != null)
            {
                return this.CheckCanRedo(transactions);
            }
            else
            {
                return default(bool);
            }
        }
        
        public Action<System.Collections.Generic.IList<MiniDB.IDBObject>, System.Collections.Generic.IList<MiniDB.Transactions.IDBTransaction>, System.Collections.Specialized.NotifyCollectionChangedEventHandler, System.Collections.Specialized.NotifyCollectionChangedEventHandler, MiniDB.PropertyChangedExtendedEventHandler> Undo { get; set; }
        
        void IUndoRedoManager.Undo(System.Collections.Generic.IList<MiniDB.IDBObject> dataToActOn, System.Collections.Generic.IList<MiniDB.Transactions.IDBTransaction> transactions, System.Collections.Specialized.NotifyCollectionChangedEventHandler dataChangedHandler, System.Collections.Specialized.NotifyCollectionChangedEventHandler transactionsChangedHandler, MiniDB.PropertyChangedExtendedEventHandler propertyChangedHandler)
        {
            if (this.Undo != null)
            {
                this.Undo(dataToActOn, transactions, dataChangedHandler, transactionsChangedHandler, propertyChangedHandler);
            }
        }
        
        public Action<System.Collections.ObjectModel.Collection<MiniDB.IDBObject>, System.Collections.ObjectModel.Collection<MiniDB.Transactions.IDBTransaction>, System.Collections.Specialized.NotifyCollectionChangedEventHandler, System.Collections.Specialized.NotifyCollectionChangedEventHandler, MiniDB.PropertyChangedExtendedEventHandler> Redo { get; set; }
        
        void IUndoRedoManager.Redo(System.Collections.ObjectModel.Collection<MiniDB.IDBObject> dataToActOn, System.Collections.ObjectModel.Collection<MiniDB.Transactions.IDBTransaction> transactions, System.Collections.Specialized.NotifyCollectionChangedEventHandler dataChangedHandler, System.Collections.Specialized.NotifyCollectionChangedEventHandler transactionsChangedHandler, MiniDB.PropertyChangedExtendedEventHandler propertyChangedHandler)
        {
            if (this.Redo != null)
            {
                this.Redo(dataToActOn, transactions, dataChangedHandler, transactionsChangedHandler, propertyChangedHandler);
            }
        }
        
    }
}
