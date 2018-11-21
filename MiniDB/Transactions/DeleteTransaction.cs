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
    }
}
