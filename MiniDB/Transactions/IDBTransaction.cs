using Newtonsoft.Json;
using System;

namespace MiniDB.Transactions
{
    public interface IDBTransaction : IDBObject
    {
        bool? Active { get; }

        ID ChangedItemID { get; }

        DBTransactionType DBTransactionType { get; }

        [JsonProperty]
        System.DateTime Transaction_timestamp { get; }

        string ChangedFieldName { get; }

        object OldValue { get; }

        object NewValue { get; }
    }
}
