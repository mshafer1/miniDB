using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB.Transactions
{
    class DBTransactionInfo : IDBTransaction
    {
        // provide contstructor and properties for NewtonSoft to construct this.
        [JsonConstructor]
        internal DBTransactionInfo()
        {

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


        public static IDBTransaction GetDBTransaction(DBTransactionInfo self)
        {
            switch(self.DBTransactionType)
            {
                case (DBTransactionType.Add):
                    return new AddTransaction(self);
                case (DBTransactionType.Delete):
                    return new DeleteTransaction(self);
                case (DBTransactionType.Modify):
                    return new ModifyTransaction(self);
                default:
                    throw new DBException($"Cannot create transaction of type {self.DBTransactionType}");
            }
        }


        public event PropertyChangedExtendedEventHandler PropertyChangedExtended;
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
