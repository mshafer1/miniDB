namespace MiniDB.Transactions
{
    public interface IWholeItemTransaction : IDBTransaction
    {
        IDBObject TransactedItem { get; }
    }
}