﻿using Newtonsoft.Json;


namespace MiniDB.Transactions
{
    abstract class BaseDBTransaction : BaseDBObject, IDBTransaction
    {
        public BaseDBTransaction() : base()
        {
            this.Transaction_timestamp = System.DateTime.Now;
            this.Active = true;
        }

        public BaseDBTransaction(IDBTransaction other) : base(other.ID)
        {
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
        /// Gets the system timestamp that this transaction occured at.
        /// </summary>
        [JsonProperty]
        public System.DateTime Transaction_timestamp { get; private set; }

        /// <summary>
        /// Gets or sets whether or not this transaction has been reversed (undone for most transactions, redone for undo transactions).
        /// </summary>
        public bool? Active { get => this.Get(); set => this.Set(value); }

        public abstract string ChangedFieldName { get; set; }

        public abstract object OldValue { get; set; }

        public abstract object NewValue { get; set; }
    }
}
