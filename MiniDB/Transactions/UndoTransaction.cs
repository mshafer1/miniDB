using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB.Transactions
{
    public class UndoTransaction : BaseDBTransaction
    {
        public UndoTransaction(IDBObject transactedItem, DBTransactionType subTransactionType) : base(transactedItem.ID)
        {
            this.TransactedItem = transactedItem;
            this.SubDBTransactionType = subTransactionType;
        }

        public UndoTransaction(IDBTransaction other) : base(other)
        {
            if (other.DBTransactionType != this.DBTransactionType)
            {
                throw new DBException($"Attempted to create class of type {nameof(AddTransaction)}, but parameter used of type {other.DBTransactionType}");
            }
        }

        public override DBTransactionType DBTransactionType => DBTransactionType.Undo;

        public DBTransactionType SubDBTransactionType { get; }

        public IDBObject TransactedItem { get; }

        public override IDBTransaction revert(IList<IDBObject> objects, PropertyChangedExtendedEventHandler notifier)
        {
            IDBTransaction result = null;

            if(this.SubDBTransactionType == DBTransactionType.Add)
            {
                // redo an add
                if(this.TransactedItem == null)
                {
                    throw new DBCannotRedoException($"Cannot find item to re-add");
                }
                var transactedItem = this.TransactedItem;
                transactedItem.PropertyChangedExtended += notifier;

                objects.Add(transactedItem);

                result = new RedoTransaction(transactedItem, DBTransactionType.Add);
            }
            else
            {
                throw new NotImplementedException("TODO: implement rest of revert undo");
            }
            

            if(result == null)
            {
                throw new DBCannotRedoException($"Failure attempting to rever Undo proocedure: {this}");
            }

            this.Active = false;
            return result;
        }
    }
}
