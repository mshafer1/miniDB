using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB.Transactions
{
    class AddTransaction : BaseDBTransaction
    {
        public AddTransaction() : base()
        { }

        public AddTransaction(IDBTransaction other) : base(other)
        {
            if (other.DBTransactionType != this.DBTransactionType)
            {
                throw new DBException($"Attempted to create class of type {nameof(AddTransaction)}, but parameter used of type {other.DBTransactionType}");
            }
        }

        public override DBTransactionType DBTransactionType => DBTransactionType.Add;

        public override string ChangedFieldName { get; set; }
        public override object OldValue { get; set; }
        public override object NewValue { get; set; }
    }
}
