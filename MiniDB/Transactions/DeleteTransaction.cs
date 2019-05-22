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

        public DeleteTransaction(IDBObject transactedItem) : base(transactedItem.ID)
        {
            this.TransactedItem = transactedItem;
        }

        public DeleteTransaction(IDBTransaction other) : base(other)
        {
           
        }

        public IDBObject TransactedItem { get; }

        public override IDBTransaction Revert(IList<IDBObject> objects, PropertyChangedExtendedEventHandler notifier)
        {
            // reverting a delete transaction means adding the item, and making an undo-add transaction
            var transacted_item = this.TransactedItem;
            if (transacted_item == null)
            {
                throw new DBCannotUndoException($"Failed to undo delete of Null object");
            }

            objects.Add(transacted_item);

            return new UndoTransaction(transacted_item, DBTransactionType.Add);
        }
    }
}
