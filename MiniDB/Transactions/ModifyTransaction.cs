using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB.Transactions
{
    class ModifyTransaction : BaseDBTransaction
    {
        public ModifyTransaction() : base()
        { }

        public ModifyTransaction(IDBTransaction other) : base(other)
        {
            if (other.DBTransactionType != this.DBTransactionType)
            {
                throw new DBException($"Attempted to create class of type {nameof(ModifyTransaction)}, but parameter used of type {other.DBTransactionType}");
            }
        }

        public override DBTransactionType DBTransactionType => DBTransactionType.Modify;
    }
}
