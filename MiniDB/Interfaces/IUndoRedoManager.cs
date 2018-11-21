using MiniDB.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB.Interfaces
{
    public interface IUndoRedoManager
    {
        bool CheckCanUndo(IEnumerable<IDBTransaction> transactions);

        bool CheckCanRedo(IEnumerable<IDBTransaction> transactions);

        IDBTransaction Undo(IEnumerable<IDBObject> dataToActOn, IEnumerable<IDBTransaction> transactions);

        IDBTransaction Redo(IEnumerable<IDBObject> dataToActOn, IEnumerable<IDBTransaction> transactions);
    }
}
