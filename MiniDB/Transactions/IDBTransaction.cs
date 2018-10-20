using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    public interface IDBTransaction
    {
        DBTransactionType DBTransactionType { get; }
    }
}
