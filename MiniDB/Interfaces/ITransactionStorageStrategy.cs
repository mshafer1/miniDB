using System.Collections.ObjectModel;

using MiniDB.Transactions;

namespace MiniDB.Interfaces
{
    public interface ITransactionStorageStrategy
    {
        /// <summary>
        /// load the transactions db
        /// </summary>
        /// <param name="filename">the filename/path that the db is stored in</param>
        /// <returns>new DataBase of <see cref="DBTransaction{T}" /></returns>
        ObservableCollection<IDBTransaction> GetTransactionsCollection(string filename);

        void CacheTransactions(ObservableCollection<IDBTransaction> dBTransactions, string filename);
    }
}
