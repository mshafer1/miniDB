namespace MiniDB.Transactions
{
    public abstract class WholeItemTransaction : BaseDBTransaction, IWholeItemTransaction
    {
        public WholeItemTransaction(IDBObject transactedObject) : base(transactedObject.ID)
        {
            this.TransactedItem = transactedObject;
        }

        public WholeItemTransaction(IWholeItemTransaction other) : base(other)
        {
            this.TransactedItem = other.TransactedItem;
        }

        public IDBObject TransactedItem { get; }
    }
}
