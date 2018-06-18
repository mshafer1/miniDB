using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    // TODO: add resolve transaction - cannot be undone
    public enum TransactionType
    {
        Add,
        Delete,
        Modify,
        Undo,
        Redo,
        Unknown
    }

    public class DBTransaction<T> : DatabaseObject where T : DatabaseObject
    {
        public DBTransaction() : base()
        {
            ID = nextAvailableID;
            Transaction_timestamp = DateTime.Now;
            this.Active = true;
            nextAvailableID++;
        }

        internal DBTransaction(DBTransaction<T> inTransaction) : base()
        {
            ID = inTransaction.ID;
            Transaction_timestamp = inTransaction.Transaction_timestamp;
            Transacted_item = inTransaction.Transacted_item;
            TransactionType = inTransaction.TransactionType;
            Item_ID = inTransaction.Item_ID;
            changed_property = inTransaction.changed_property;
            property_new = inTransaction.property_new;
            property_old = inTransaction.property_old;
            
            // needed? - keep until collision detecting is implemented
            nextAvailableID++;
        }

       

        #region transacted object properties
        public ID Item_ID { get; set; }
        public T Transacted_item { get; set; }
        public string changed_property { get; set; }
        public object property_old { get; set; }
        public object property_new { get; set; }
        #endregion

        #region transaction properties
        [JsonProperty()]
        public int ID { get; private set; } // switch to using default ID??
        [JsonProperty()]
        public DateTime Transaction_timestamp { get; private set; }

        public TransactionType TransactionType { get => this.Get(); set => this.Set(value); }
        public bool? Active { get => this.Get(); set => this.Set(value); }
        #endregion

        private static int nextAvailableID = 0;

        public const string ITEM_REMOVED = "ITEM_REMOVED";
        public const string ITEM_ADDED = "ITEM_ADDED";
    }
}
