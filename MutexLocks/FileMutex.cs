using System;

namespace MutexLocks
{
    class FileMutex : IDisposable, IMutex
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public MutexObject Get()
        {
            throw new NotImplementedException();
        }

        public void Lock(string name)
        {
            throw new NotImplementedException();
        }

        public void Unlock()
        {
            throw new NotImplementedException();
        }
    }
}
