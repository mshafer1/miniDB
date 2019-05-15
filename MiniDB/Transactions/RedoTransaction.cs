using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB.Transactions
{
    public class RedoTransaction : BaseDBTransaction
    {
        public RedoTransaction() : base()
        { }

        public RedoTransaction(IDBTransaction other) : base(other)
        {
            if (other.DBTransactionType != this.DBTransactionType)
            {
                throw new DBException($"Attempted to create class of type {nameof(AddTransaction)}, but parameter used of type {other.DBTransactionType}");
            }
        }

        public override DBTransactionType DBTransactionType => DBTransactionType.Redo;

        public override IDBTransaction revert(IList<IDBObject> objects)
        {
            throw new NotImplementedException();
        }
    }
}
