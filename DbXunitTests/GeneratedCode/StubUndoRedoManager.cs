// this file was created by AutoImplement
using System;

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
        
        public Func<System.Collections.Generic.IEnumerable<MiniDB.IDBObject>, System.Collections.Generic.IEnumerable<MiniDB.Transactions.IDBTransaction>, MiniDB.Transactions.IDBTransaction> Undo { get; set; }
        
        MiniDB.Transactions.IDBTransaction IUndoRedoManager.Undo(System.Collections.Generic.IEnumerable<MiniDB.IDBObject> dataToActOn, System.Collections.Generic.IEnumerable<MiniDB.Transactions.IDBTransaction> transactions)
        {
            if (this.Undo != null)
            {
                return this.Undo(dataToActOn, transactions);
            }
            else
            {
                return default(MiniDB.Transactions.IDBTransaction);
            }
        }
        
        public Func<System.Collections.Generic.IEnumerable<MiniDB.IDBObject>, System.Collections.Generic.IEnumerable<MiniDB.Transactions.IDBTransaction>, MiniDB.Transactions.IDBTransaction> Redo { get; set; }
        
        MiniDB.Transactions.IDBTransaction IUndoRedoManager.Redo(System.Collections.Generic.IEnumerable<MiniDB.IDBObject> dataToActOn, System.Collections.Generic.IEnumerable<MiniDB.Transactions.IDBTransaction> transactions)
        {
            if (this.Redo != null)
            {
                return this.Redo(dataToActOn, transactions);
            }
            else
            {
                return default(MiniDB.Transactions.IDBTransaction);
            }
        }
        
    }
}
