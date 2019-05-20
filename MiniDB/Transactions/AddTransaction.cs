using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB.Transactions
{
    public class AddTransaction : WholeItemTransaction
    {
        public AddTransaction(IDBObject addedObject) : base(addedObject)
        {
        }

        public AddTransaction(IWholeItemTransaction other) : base(other)
        {
        }

        public override DBTransactionType DBTransactionType => DBTransactionType.Add;

        public override IDBTransaction revert(IList<IDBObject> objects, PropertyChangedExtendedEventHandler notifier)
        {
            // reverting an Add Transaction means removing the item and creating a Delete transaction
            IDBObject transactedObject = objects.FirstOrDefault(entry => entry.ID == this.ChangedItemID);

            if(transactedObject == null)
            {
                throw new DBCannotUndoException($"Failed to find item wit ID {this.ChangedItemID} to remove");
            }

            objects.Remove(transactedObject);
            this.Active = false;

            // return an Undo-Add transaction object
            return new UndoTransaction(transactedObject, DBTransactionType.Add);
        }
    }
}
