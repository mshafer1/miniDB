using MiniDB.Transactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB.Interfaces
{
    public interface ITransactionStorageStrategy
    {
        /// <summary>
        /// load the transactions db
        /// </summary>
        /// <param name="transactions_filename">the filename/path that the db is stored in</param>
        /// <returns>new DataBase of <see cref="DBTransaction{T}" /></returns>
        ObservableCollection<IDBTransaction> _getTransactionsCollection(string filename);

        void _cacheTransactions(ObservableCollection<IDBTransaction> dBTransactions, string filename);
    }
}
