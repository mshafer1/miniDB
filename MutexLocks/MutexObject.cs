using System;

namespace MutexLocks
{
    public class MutexObject : IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Release()
        {
            this.Dispose();
        }
    }
}