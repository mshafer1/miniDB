using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    public class DBTransaction<T> : IDatabaseObject where T : IDatabaseObject
    {
    }
}
