namespace MiniDB.Transactions
{
    public interface IModifyTransaction : IDBTransaction
    {
        string ChangedFieldName { get; }

        object OldValue { get; }

        object NewValue { get; }
    }
}