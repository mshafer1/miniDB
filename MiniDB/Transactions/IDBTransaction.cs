using System.Collections.Generic;

using MiniDB.Interfaces;
using Newtonsoft.Json;

namespace MiniDB.Transactions
{
    public interface IDBTransaction : IDBObject
    {
        bool? Active { get; }

        ID ChangedItemID { get; }

        DBTransactionType DBTransactionType { get; }

        [JsonProperty]
        System.DateTime Transaction_timestamp { get; }

        IDBTransaction Revert(IList<IDBObject> objects, PropertyChangedExtendedEventHandler notifier);
    }
}
