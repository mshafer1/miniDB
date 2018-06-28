using Newtonsoft.Json;

namespace MiniDB
{
    /// <summary>
    /// Available types of transactions that can occur on a Database
    ///   TODO: add resolve transaction - cannot be undone
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        /// An unkown transaction type (default)
        /// </summary>
        Unknown = -1,
        
        /// <summary>
        /// Adding an item
        /// </summary>
        Add,
        
        /// <summary>
        /// Removing an item
        /// </summary>
        Delete,
        
        /// <summary>
        /// Changing an item
        /// </summary>
        Modify,
        
        /// <summary>
        /// An undo change
        /// </summary>
        Undo,

        /// <summary>
        /// An redo change
        /// </summary>
        Redo,
    }

    /// <summary>
    /// Transaction class to save information about changes to the Database of type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DBTransaction<T> : DatabaseObject where T : DatabaseObject
    {
        /// <summary>
        /// Key to use when removing items
        /// </summary>
        public const string ItemRemovedConstKey = "ITEM_REMOVED";

        /// <summary>
        /// Key to use on added items
        /// </summary>
        public const string ItemAddConstKey = "ITEM_ADDED";

        /// <summary>
        /// Keep track of the next available transaction ID
        /// </summary>
        private static int nextAvailableID = 0;

        /// <summary>
        /// Initializes a new instance of <see cref="DBTransaction{T}" /> class.
        /// </summary>
        public DBTransaction() : base()
        {
            this.ID = nextAvailableID;
            this.Transaction_timestamp = System.DateTime.Now;
            this.Active = true;
            nextAvailableID++;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DBTransaction{T}" /> class.
        ///   Copy constructor by value
        /// </summary>
        /// <param name="inTransaction">The transaction to copy</param>
        internal DBTransaction(DBTransaction<T> inTransaction) : base()
        {
            this.ID = inTransaction.ID;
            this.Transaction_timestamp = inTransaction.Transaction_timestamp;
            this.Transacted_item = inTransaction.Transacted_item;
            this.TransactionType = inTransaction.TransactionType;
            this.Item_ID = inTransaction.Item_ID;
            this.Changed_property = inTransaction.Changed_property;
            this.Property_new = inTransaction.Property_new;
            this.Property_old = inTransaction.Property_old;
            
            // needed? - keep until collision detecting is implemented
            nextAvailableID++;
        }

        #region transacted object properties
        /// <summary>
        /// Gets or sets the ID of the item that was acted on
        /// </summary>
        public ID Item_ID { get; set; }

        /// <summary>
        /// Gets or sets the object that was acted on
        /// </summary>
        public T Transacted_item { get; set; }

        /// <summary>
        /// Gets or sets what property was changed
        /// </summary>
        public string Changed_property { get; set; }

        /// <summary>
        /// Gets or sets the old property value
        /// </summary>
        public object Property_old { get; set; }

        /// <summary>
        /// Gets or sets the new property value
        /// </summary>
        public object Property_new { get; set; }
        #endregion

        #region transaction properties
        /// <summary>
        /// Gets an ID of this transaction (hides default ID, as we want this to be incremental).
        /// </summary>
        [JsonProperty]
        public int ID { get; private set; } // switch to using default ID??

        /// <summary>
        /// Gets the system timestamp that this transaction occured at.
        /// </summary>
        [JsonProperty]
        public System.DateTime Transaction_timestamp { get; private set; }

        /// <summary>
        /// Gets or sets the type of transaction (Add, delete, modify, etc.) of type <see cref="TransactionType"/>.
        /// </summary>
        public TransactionType TransactionType { get => this.Get(); set => this.Set(value); }

        /// <summary>
        /// Gets or sets whether or not this transaction has been reversed (undone for most transactions, redone for undo transactions).
        /// </summary>
        public bool? Active { get => this.Get(); set => this.Set(value); }
        #endregion
    }
}
