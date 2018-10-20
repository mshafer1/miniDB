﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB.Transactions
{
    class DeleteTransaction : WholeItemTransaction
    {
        public override DBTransactionType DBTransactionType => DBTransactionType.Delete;
    }
}
