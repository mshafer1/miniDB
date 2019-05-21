using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB.Transactions
{
    public class RedoTransaction : BaseDBTransaction
    {
        public RedoTransaction(IDBObject transactedObject, DBTransactionType subTransactionType) : base(transactedObject.ID)
        {
            this.SubDBTransactionType = subTransactionType;
            this.TransactedItem = transactedObject;
        }

        public RedoTransaction(IDBTransaction other) : base(other)
        {
            if (other.DBTransactionType != this.DBTransactionType)
            {
                throw new DBException($"Attempted to create class of type {nameof(AddTransaction)}, but parameter used of type {other.DBTransactionType}");
            }
        }

        public override DBTransactionType DBTransactionType => DBTransactionType.Redo;

        public DBTransactionType SubDBTransactionType { get; set; }

        public IDBObject TransactedItem { get; set; }

        public override IDBTransaction revert(IList<IDBObject> objects, PropertyChangedExtendedEventHandler notifier)
        {
            IDBTransaction result = null;

            if (this.SubDBTransactionType == DBTransactionType.Add)
            {
                // revert an add
                if (this.TransactedItem == null)
                {
                    throw new DBCannotUndoException($"Cannot find item to re-remove");
                }
                var transactedItem = objects.FirstOrDefault(dbItem => dbItem.ID == this.TransactedItem.ID);
                if(transactedItem == null)
                {
                    throw new DBCannotUndoException($"Cannot find item with ID: {this.TransactedItem.ID}");
                }
                transactedItem.PropertyChangedExtended -= notifier;

                objects.Remove(transactedItem);

                result = new UndoTransaction(transactedItem, DBTransactionType.Add);
            }
            else
            {
                throw new NotImplementedException("TODO: implement rest of revert undo");
            }


            if (result == null)
            {
                throw new DBCannotRedoException($"Failure attempting to revert Redo procedure: {this}");
            }

            this.Active = false;
            return result;
        }
    }
}
