using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB.Transactions
{
    class DeleteTransaction : BaseDBTransaction
    {
        public override DBTransactionType DBTransactionType => DBTransactionType.Delete;

        public DeleteTransaction() : base()
        { }

        public DeleteTransaction(IDBTransaction other) : base(other)
        {
            if (other.DBTransactionType != this.DBTransactionType)
            {
                throw new DBException($"Attempted to create class of type {nameof(DeleteTransaction)}, but parameter used of type {other.DBTransactionType}");
            }
        }

        public IDBObject TransactedItem { get; set; }

        public override IDBTransaction revert(IList<IDBObject> objects, PropertyChangedExtendedEventHandler notifier)
        {
            // reverting a delete transaction means adding the item, and making an undo-add transaction
            var transacted_item = this.TransactedItem;
            if (transacted_item == null)
            {
                throw new DBCannotUndoException($"Failed to undo delete of Null object");
            }

            objects.Add(transacted_item);

            return new UndoTransaction()
            {
                SubDBTransactionType = DBTransactionType.Add,
                TransactedItem = transacted_item,
                ChangedItemID = transacted_item.ID,
            };
        }
    }
}
