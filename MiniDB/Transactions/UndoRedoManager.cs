using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using MiniDB.Interfaces;

namespace MiniDB.Transactions
{
    // this class is responsible for knowing how to handle undos and redos for the DB
    public class UndoRedoManager : IUndoRedoManager
    {
        private readonly ITransactionStorageStrategy storageStrategy;
        private readonly ObservableCollection<IDBTransaction> transactions_DB;
        private readonly string transactions_filename;

        public UndoRedoManager(ITransactionStorageStrategy storageStrategy, string filename)
        {
            this.storageStrategy = storageStrategy;
            this.transactions_filename = filename;

            this.transactions_DB = storageStrategy.GetTransactionsCollection(this.transactions_filename);
            this.transactions_DB.CollectionChanged += this.DataBase_TransactionsChanged;
        }

        public bool CheckCanUndo()
        {
            var transactionsCount = this.transactions_DB.Count();
            var undoTransactionsCount = this.transactions_DB.Count(x => x.DBTransactionType == DBTransactionType.Undo);

            return transactionsCount > 0 && transactionsCount > (2 * undoTransactionsCount);
        }

        public bool CheckCanRedo()
        {
            // TODOne: this should be number of immediate redo's is less than number of next immediate undo's
            var redos_count = this.CountRecentTransactions(DBTransactionType.Redo, this.transactions_DB);
            Func<IDBTransaction, bool> matcher = x => x.DBTransactionType == DBTransactionType.Undo && x.Active == true;
            var undos_count = this.CountRecentTransactions(matcher, this.transactions_DB.Skip(redos_count * 2));
            return undos_count > 0;
        }

        public void InsertTransaction(IDBTransaction transaction)
        {
            this.transactions_DB.Insert(0, transaction);
        }

        public void Undo(IList<IDBObject> dataToActOn, NotifyCollectionChangedEventHandler dataChangedHandler, PropertyChangedExtendedEventHandler propertyChangedHandler)
        {
            // inside mutex; however, not in creation, so normal catch/dispose methods should clear mutex
            if (!this.CheckCanUndo())
            {
                throw new DBCannotUndoException("Cannot undo at this time");
            }

            IDBTransaction new_transaction = null;
            using (new TransactionBlockScope(dataToActOn, this.transactions_DB, dataChangedHandler, this.DataBase_TransactionsChanged))
            {
                // get last transaction
                var last_transaction = this.GetLastTransaction(DBTransactionType.Undo, x => x.Active == true);

                new_transaction = last_transaction.Revert(dataToActOn, propertyChangedHandler);
            }

            this.transactions_DB.Insert(0, new_transaction);
        }

        public void Redo(Collection<IDBObject> dataToActOn, NotifyCollectionChangedEventHandler dataChangedHandler, PropertyChangedExtendedEventHandler propertyChangedHandler)
        {
            // inside mutex; however, not in creation, so normal catch/dispose methods should clear mutex
            if (!this.CheckCanRedo())
            {
                throw new DBCannotRedoException("Cannot undo at this time");
            }

            IDBTransaction new_transaction = null;
            using (new TransactionBlockScope(dataToActOn, this.transactions_DB, dataChangedHandler, this.DataBase_TransactionsChanged))
            {
                // get last transaction
                var last_transaction = this.GetLastTransaction(DBTransactionType.Redo, x => x.Active == true);

                new_transaction = last_transaction.Revert(dataToActOn, propertyChangedHandler);
            }

            this.transactions_DB.Insert(0, new_transaction);
        }

        private static void DeregisterListener(IDBObject item, PropertyChangedExtendedEventHandler listener)
        {
            item.PropertyChangedExtended -= listener;
        }

        private static void ReregisterListener(IDBObject item, PropertyChangedExtendedEventHandler listener)
        {
            // first make sure it's not registered already
            DeregisterListener(item, listener);

            item.PropertyChangedExtended += listener;
        }

        private static void DeregisterListener<T>(ObservableCollection<T> list, NotifyCollectionChangedEventHandler listener)
        {
            list.CollectionChanged -= listener;
        }

        private static void ReregisterListener<T>(ObservableCollection<T> list, NotifyCollectionChangedEventHandler listener)
        {
            // first make sure it's not registered already
            DeregisterListener(list, listener);

            list.CollectionChanged += listener;
        }

        /// <summary>
        /// Use reflection to set reverse the last transaction on the transacted item
        /// </summary>
        /// <param name="last_transaction">what happened that we're changing back</param>
        /// <param name="transactedItem">the item to act on</param>
        private static void SetProperty(ModifyTransaction last_transaction, IDBObject transactedItem)
        {
            // inside mutex; however, not in creation, so normal catch/dispose methods should clear mutex

            // TODO: redo with https://stackoverflow.com/a/13270302
            var properties = last_transaction.ChangedFieldName.Split('.');
            object lastObject = transactedItem;
            System.Reflection.PropertyInfo currentProperty = null;

            foreach (var attribute in properties)
            {
                // have currentObject lag behind since it should reflect the object the last property is on
                if (currentProperty != null)
                {
                    lastObject = currentProperty.GetValue(lastObject);
                }

                // get the property information based on the type
                if (!attribute.Contains("["))
                {
                    currentProperty = lastObject.GetType().GetProperty(attribute, BindingFlags.Public | BindingFlags.Instance);
                }
                else
                {
                    if (!attribute.Contains("]"))
                    {
                        throw new DBCannotUndoException($"Property name {attribute} contains unmatched '['");
                    }

                    if (attribute.IndexOf('[') != 0)
                    {
                        var propertyName = attribute.Substring(0, attribute.IndexOf('['));
                        currentProperty = lastObject.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance); // get the dictionary property
                        lastObject = currentProperty.GetValue(lastObject); // get the dictionary object
                        if (lastObject == null)
                        {
                            throw new DBCannotUndoException($"Cannot access property {propertyName} on {lastObject}");
                        }
                    }

                    lastObject = GetIndexedObject(lastObject, attribute);
                    currentProperty = null;
                }
            }

            object oldVal = GetOldValue(last_transaction, properties, lastObject, currentProperty);

            // Set the value of the property
            currentProperty.SetValue(lastObject, oldVal, null);
        }

        private static object GetIndexedObject(object lastObject, string attribute)
        {
            Type t = lastObject.GetType();
            if (!t.IsGenericType || t.GetGenericTypeDefinition() != typeof(Dictionary<,>))
            {
                throw new DBCannotUndoException($"Property {lastObject} is not a dictionary, but was used with indexers");
            }

            var r = new Regex(@"\[.+\]");
            Match m = r.Match(attribute);
            if (!m.Success)
            {
                // possible??
                throw new DBCannotUndoException($"Cannot undo property: {attribute}");
            }

            var keyType = t.GetGenericArguments()[0];
            var valueType = t.GetGenericArguments()[1];

            // store key without square brackets
            var key = m.Value.Substring(1);
            key = key.Substring(0, key.Length - 1);
            var keyObject = Convert.ChangeType(key, keyType);

            // currentProperty = Convert.ChangeType(lastObject, keyType);
            var p1 = t.GetProperty("Item"); // get indexer property
            lastObject = p1.GetValue(lastObject, new object[] { keyObject });
            return lastObject;
        }

        private static object GetOldValue(ModifyTransaction last_transaction, string[] properties, object lastObject, PropertyInfo currentProperty)
        {
            if (currentProperty == null)
            {
                throw new DBCannotUndoException($"Cannot access property {properties.First()} on {lastObject}");
            }

            // find the property type
            Type propertyType = currentProperty.PropertyType;

            // Convert.ChangeType does not handle conversion to nullable types
            //  if the property type is nullable, we need to get the underlying type of the property
            var targetType = IsNullableType(propertyType) ? Nullable.GetUnderlyingType(propertyType) : propertyType;

            // Returns an System.Object with the specified System.Type and whose value is
            //   equivalent to the specified object.
            object oldVal = null;
            if (targetType.IsEnum)
            {
                // need converter for int to enum
                oldVal = Enum.ToObject(targetType, last_transaction.OldValue);
            }
            else
            {
                oldVal = Convert.ChangeType(last_transaction.OldValue, targetType);
            }

            return oldVal;
        }

        /// <summary>
        /// Determine if the object is a nullable type
        /// </summary>
        /// <param name="type">the type in question</param>
        /// <returns>true if nullable</returns>
        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
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
                    .Select(x => x.index)                                     // select the index
                    .FirstOrDefault()                                         // return first index
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

        private void DataBase_TransactionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.storageStrategy.CacheTransactions(this.transactions_DB, this.transactions_filename);
        }

        /// <summary>
        /// Get last transaction skipping all transaction with a type in notTransactionType and matches matcher
        /// </summary>
        /// <param name="notTransactionType">Transaction type to skip</param>
        /// <param name="matcher">checker to determine match</param>
        /// <returns>last transaction skipping all transaction with a type in notTransactionType and matches matcher</returns>
        private IDBTransaction GetLastTransaction(DBTransactionType notTransactionType, Func<IDBTransaction, bool> matcher)
        {
            IDBTransaction last_transaction = null;
            IEnumerable<IDBTransaction> temp_list = this.transactions_DB;
            do
            {
                bool first_matches = false;

                // is first in list active?
                if (temp_list != null & temp_list.Count() > 0)
                {
                    first_matches = matcher(temp_list.First());
                }

                if (temp_list == null || temp_list.Count() == 0)
                {
                    // ran out of options
                    throw new DBCannotUndoException("Cannot find transaction");
                }

                if (temp_list.FirstOrDefault()?.DBTransactionType == notTransactionType)
                {
                    // transaction type to skip past
                    var count = this.CountRecentTransactions(notTransactionType, temp_list);
                    temp_list = temp_list.Skip(count * 2);
                }
                else if (first_matches)
                {
                    last_transaction = temp_list.FirstOrDefault();
                }
                else
                {
                    temp_list = temp_list.Skip(1);
                }
            }
            while (last_transaction == null || last_transaction.DBTransactionType == notTransactionType);
            return last_transaction;
        }

        private class TransactionBlockScope : IDisposable
        {
            private readonly ObservableCollection<IDBObject> data;
            private readonly ObservableCollection<IDBTransaction> transactions;

            private readonly NotifyCollectionChangedEventHandler dataChangedHandler;
            private readonly NotifyCollectionChangedEventHandler transactionsChangedHandler;

            public TransactionBlockScope(IList<IDBObject> dataToActOn, IList<IDBTransaction> transactions, NotifyCollectionChangedEventHandler dataChangedHandler, NotifyCollectionChangedEventHandler transactionsChangedHandler)
            {
                this.data = (ObservableCollection<IDBObject>)dataToActOn;
                this.transactions = (ObservableCollection<IDBTransaction>)transactions;

                this.dataChangedHandler = dataChangedHandler;
                this.transactionsChangedHandler = transactionsChangedHandler;

                DeregisterListener(this.data, this.dataChangedHandler);
                DeregisterListener(this.transactions, this.transactionsChangedHandler);
            }

            public void Dispose()
            {
                ReregisterListener(this.data, this.dataChangedHandler);
                ReregisterListener(this.transactions, this.transactionsChangedHandler);
            }
        }
    }
}
