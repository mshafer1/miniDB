using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB.Transactions
{
    class ModifyTransaction : BaseDBTransaction
    {
        public override DBTransactionType DBTransactionType => DBTransactionType.Modify;

        public string Changed_Property { get; set; }

        public object Property_new { get; set; }

        public object Property_old { get; set; }
    }
}
