using Newtonsoft.Json;


namespace MiniDB.Transactions
{
    abstract class BaseDBTransaction : BaseDBObject, IDBTransaction, IDatabaseObject
    {
        public BaseDBTransaction() : base()
        {
            this.Transaction_timestamp = System.DateTime.Now;
            this.Active = true;
        }

        public abstract DBTransactionType DBTransactionType { get; }

        /// <summary>
        /// Gets or sets the ID of the item that was acted on
        /// </summary>
        public ID ChangedItemID { get; set; }

        /// <summary>
        /// Gets the system timestamp that this transaction occured at.
        /// </summary>
        [JsonProperty]
        public System.DateTime Transaction_timestamp { get; private set; }

        /// <summary>
        /// Gets or sets whether or not this transaction has been reversed (undone for most transactions, redone for undo transactions).
        /// </summary>
        public bool? Active { get => this.Get(); set => this.Set(value); }

    }
}
