using MiniDB.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB.Transactions
{
    // this class is responsible for know how to handle undos and redos for the DB
    class UndoRedoManager : IUndoRedoManager
    {
        internal UndoRedoManager()
        { }

        public bool CheckCanUndo(IEnumerable<IDBTransaction> transactions)
        {
            return transactions.Count() > 0 && transactions.Count() >
                    (2 * transactions.Count(x => x.DBTransactionType == DBTransactionType.Undo));
        }

        public bool CheckCanRedo(IEnumerable<IDBTransaction> transactions)
        {
            // TODOne: this should be number of immediate redo's is less than number of next immediate undo's
            //  bool result = true;
            var redos_count = this.CountRecentTransactions(DBTransactionType.Redo, transactions);
            Func<IDBTransaction, bool> matcher = x => x.DBTransactionType == DBTransactionType.Undo && x.Active == true;
            var undos_count = this.CountRecentTransactions(matcher, transactions.Skip(redos_count * 2));
            return undos_count > 0;
        }


        /// <summary>
        /// Count recent transactions that match the provided TransactionType
        /// </summary>
        /// <param name="transactionType">transaction type to count</param>
        /// <param name="list">(optional) the list to search (defaults to this)</param>
        /// <returns>the count of matches</returns>
        private int CountRecentTransactions(DBTransactionType transactionType, IEnumerable<IDBTransaction> list)
        {
            var first = list.FirstOrDefault();
            if (first == null)
            {
                return 0;
            }

            int count = (first.DBTransactionType == transactionType) ?
                list.Select((item, index) => new { item, index })
                    .Where(x => (x.item.DBTransactionType != transactionType)) // find non matches
                    .Select(x => x.index)                                    // select the index
                    .FirstOrDefault()                                        // return first index
                : 0;
            return count;
        }


        /// <summary>
        /// Count recent transactions that match the predicate
        /// </summary>
        /// <param name="predicate">function that takes a transaction and returns if it matches or not</param>
        /// <param name="list">(optional) the list to search (defaults to this)</param>
        /// <returns>the count of matches</returns>
        private int CountRecentTransactions(Func<IDBTransaction, bool> predicate, IEnumerable<IDBTransaction> list)
        {
            var first = list.FirstOrDefault();
            if (first == null)
            {
                return 0;
            }

            int count = predicate(first) ?
                list.Select((item, index) => new { item, index })
                    .Where(x => !predicate(x.item)) // find non matching
                    .Select(x => x.index)           // select its index
                    .FirstOrDefault()               // get first value (index of first non-match)
                : 0;
            return count;
        }

        /// <summary>
        /// Count the number of transactions at the start of the transactions db that are of a type in transactionTypes
        /// </summary>
        /// <param name="transactionTypes">transaction types to count</param>
        /// <param name="list">(optional) list of transactions to count in (uses this if not provided).</param>
        /// <returns>the count of transactions</returns>
        private int CountRecentTransactions(List<DBTransactionType> transactionTypes, IEnumerable<IDBTransaction> list)
        {
                var first = list.FirstOrDefault();
                if (first == null)
                {
                    return 0;
                }

                int count = transactionTypes.Contains(first.DBTransactionType) ?
                    list.Select((item, index) => new { item, index })
                        .Where(x => !transactionTypes.Contains(x.item.DBTransactionType)).Select(x => x.index).FirstOrDefault()
                    : 0;
                return count;
            
        }

        public IDBTransaction Undo(IEnumerable<IDBObject> dataToActOn, IEnumerable<IDBTransaction> transactions)
        {
            throw new NotImplementedException();
        }

        public IDBTransaction Redo(IEnumerable<IDBObject> dataToActOn, IEnumerable<IDBTransaction> transactions)
        {
            throw new NotImplementedException();
        }
    }
}
