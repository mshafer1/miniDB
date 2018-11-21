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

        // TODO: we don't care about these, but have to have them . . . is there a cleaner way to do this?
        public override string ChangedFieldName { get; set; }
        public override object OldValue { get; set; }
        public override object NewValue { get; set; }
    }
}
