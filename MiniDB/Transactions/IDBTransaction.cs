using Newtonsoft.Json;
using System;

namespace MiniDB.Transactions
{
    public interface IDBTransaction : IDatabaseObject
    {
        bool? Active { get; }

        ID ChangedItemID { get; }

        DBTransactionType DBTransactionType { get; }

        [JsonProperty]
        System.DateTime Transaction_timestamp { get; }
    }
}
