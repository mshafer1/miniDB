using System;

namespace MutexLocks
{
    public class MutexException : Exception
    {
        public MutexException()
            : base()
        {
        }

        public MutexException(string message)
            : base(message)
        {
        }
    }
}
