using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MutexLocks
{
    public class MutexException : Exception
    {
        public MutexException() : base()
        { }

        public MutexException(string message) : base(message)
        { }
    }
}
