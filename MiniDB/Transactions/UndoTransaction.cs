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
        public UndoTransaction() : base()
        { }

        public UndoTransaction(IDBTransaction other) : base(other)
        {
            if (other.DBTransactionType != this.DBTransactionType)
            {
                throw new DBException($"Attempted to create class of type {nameof(AddTransaction)}, but parameter used of type {other.DBTransactionType}");
            }
        }

        public override DBTransactionType DBTransactionType => DBTransactionType.Undo;

        public DBTransactionType SubDBTransactionType { get; set; }

        public IDBObject TransactedItem { get; set; }

        public override IDBTransaction revert(IList<IDBObject> objects, PropertyChangedExtendedEventHandler notifier)
        {
            throw new NotImplementedException();
        }
    }
}
