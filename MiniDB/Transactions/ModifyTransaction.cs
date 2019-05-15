using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB.Transactions
{
    public class ModifyTransaction : BaseDBTransaction
    {
        public ModifyTransaction() : base()
        { }

        public ModifyTransaction(IDBTransaction other) : base(other)
        {
            if (other.DBTransactionType != this.DBTransactionType)
            {
                throw new DBException($"Attempted to create class of type {nameof(ModifyTransaction)}, but parameter used of type {other.DBTransactionType}");
            }

            this.ChangedFieldName = other.ChangedFieldName;
            this.OldValue = other.OldValue;
            this.NewValue = other.NewValue;
        }

        public override DBTransactionType DBTransactionType => DBTransactionType.Modify;

        public override string ChangedFieldName { get; set; }

        public override object OldValue { get; set; }

        public override object NewValue { get; set; }

        public override IDBTransaction revert(IList<IDBObject> objects)
        {
            throw new NotImplementedException();
        }
    }
}
