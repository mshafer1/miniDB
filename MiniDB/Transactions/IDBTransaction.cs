using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        IDBTransaction revert(IList<IDBObject> objects, PropertyChangedExtendedEventHandler notifier);
    }
}
