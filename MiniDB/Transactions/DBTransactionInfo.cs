using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using MiniDB.Interfaces;
using Newtonsoft.Json;

namespace MiniDB.Transactions
{
    public class DBTransactionInfo : IDBTransaction, IModifyTransaction, IWholeItemTransaction
    {
        // provide contstructor and properties for NewtonSoft to construct this.
        [JsonConstructor]
        internal DBTransactionInfo()
        {
            // NO-OP
        }

        event PropertyChangedExtendedEventHandler INotifyPropertyChangedExtended.PropertyChangedExtended
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        [JsonProperty]
        public bool? Active { get; internal set; }

        [JsonProperty]
        public ID ChangedItemID { get; internal set; }

        [JsonProperty]
        public DBTransactionType DBTransactionType { get; internal set; }

        [JsonProperty]
        public DateTime Transaction_timestamp { get; internal set; }

        [JsonProperty]
        public ID ID { get; internal set; }

        [JsonProperty]
        public string ChangedFieldName { get; internal set; }

        [JsonProperty]
        public object OldValue { get; internal set; }

        [JsonProperty]
        public object NewValue { get; internal set; }

        [JsonProperty]
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1300:ElementShouldBeginWithAnUpperCaseLetter", Justification = "Would collide with cast property that exposes this")]
        public object _transactedItem { get; internal set; }

        public IDBObject TransactedItem
        {
            get { return (IDBObject)this._transactedItem; }
        }

        public static IDBTransaction GetDBTransaction(DBTransactionInfo self)
        {
            switch (self.DBTransactionType)
            {
                case DBTransactionType.Add:
                    return new AddTransaction(self);
                case DBTransactionType.Delete:
                    return new DeleteTransaction(self);
                case DBTransactionType.Modify:
                    return new ModifyTransaction(self);
                default:
                    throw new DBException($"Cannot create transaction of type {self.DBTransactionType}");
            }
        }

        public IDBTransaction Revert(IList<IDBObject> objects, PropertyChangedExtendedEventHandler notifier)
        {
            throw new NotImplementedException();
        }
    }
}
