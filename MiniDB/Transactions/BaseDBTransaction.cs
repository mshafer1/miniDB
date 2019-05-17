using System.Collections.Generic;
using Newtonsoft.Json;


namespace MiniDB.Transactions
{
    public abstract class BaseDBTransaction : BaseDBObject, IDBTransaction
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
        public bool? Active { get => this.Get(); protected set => this.Set(value); }

        // TODO: we don't always care about these, but have to have them . . . is there a cleaner way to do this?
        public virtual string ChangedFieldName { get; set; }

        public virtual object OldValue { get; set; }

        public virtual object NewValue { get; set; }

        public abstract IDBTransaction revert(IList<IDBObject> objects, PropertyChangedExtendedEventHandler notifier);
    }
}
