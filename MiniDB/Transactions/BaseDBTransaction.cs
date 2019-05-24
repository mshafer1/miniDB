using System.Collections.Generic;
using Newtonsoft.Json;

using MiniDB.Interfaces;


namespace MiniDB.Transactions
{
    public abstract class BaseDBTransaction : BaseDBObject, IDBTransaction
    {
        public BaseDBTransaction(ID changedItemID) : base()
        {
            this.Transaction_timestamp = System.DateTime.Now;
            this.Active = true;
            this.ChangedItemID = changedItemID;
        }

        public BaseDBTransaction(IDBTransaction other) : this(other.ID)
        {
            if (other.DBTransactionType != this.DBTransactionType)
            {
                throw new DBException($"Attempted to create class of type {this.DBTransactionType}, but parameter used of type {other.DBTransactionType}");
            }

            this.Transaction_timestamp = other.Transaction_timestamp;
            this.Active = other.Active;
            this.ChangedItemID = other.ChangedItemID;
        }

        public abstract DBTransactionType DBTransactionType { get; }

        /// <summary>
        /// Gets or sets the ID of the item that was acted on
        /// </summary>
        public ID ChangedItemID { get => this.Get(); set => this.Set(value); }

        /// <summary>
        /// Gets the system timestamp that this transaction occurred at.
        /// </summary>
        [JsonProperty]
        public System.DateTime Transaction_timestamp { get; private set; }

        /// <summary>
        /// Gets or sets whether or not this transaction has been reversed (undone for most transactions, redone for undo transactions).
        /// </summary>
        public bool? Active { get => this.Get(); protected set => this.Set(value); }

        public abstract IDBTransaction Revert(IList<IDBObject> objects, PropertyChangedExtendedEventHandler notifier);
    }
}
