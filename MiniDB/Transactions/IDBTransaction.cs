using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    internal interface IDBTransaction
    {
        DBTransactionType DBTransactionType { get; }
    }
}
