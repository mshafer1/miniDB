using MiniDB.Transactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB.Interfaces
{
    public interface IUndoRedoManager
    {
        bool CheckCanUndo(IEnumerable<IDBTransaction> transactions);

        bool CheckCanRedo(IEnumerable<IDBTransaction> transactions);

        void Undo(IList<IDBObject> dataToActOn, IList<IDBTransaction> transactions, NotifyCollectionChangedEventHandler dataChangedHandler, NotifyCollectionChangedEventHandler transactionsChangedHandler, PropertyChangedExtendedEventHandler propertyChangedHandler);

        void Redo(Collection<IDBObject> dataToActOn, Collection<IDBTransaction> transactions, NotifyCollectionChangedEventHandler dataChangedHandler, NotifyCollectionChangedEventHandler transactionsChangedHandler, PropertyChangedExtendedEventHandler propertyChangedHandler);
    }
}
