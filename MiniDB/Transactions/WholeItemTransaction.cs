using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB.Transactions
{
    abstract class WholeItemTransaction : BaseDBTransaction
    {
        IDBObject TransactedItem { get; set; }
    }
}
