using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB.Transactions
{
    class AddTransaction : WholeItemTransaction
    {
        public override DBTransactionType DBTransactionType => DBTransactionType.Add;
    }
}
