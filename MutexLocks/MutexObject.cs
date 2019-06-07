using System;

namespace MutexLocks
{
    public class MutexObject : IDisposable
    {
        private readonly IDisposable lockObject;

        private bool alreadyDisposed;

        public MutexObject(IDisposable lockObject)
        {
            this.lockObject = lockObject;
            this.alreadyDisposed = false;
        }

        public void Dispose()
        {
            if(!this.alreadyDisposed)
            {
                this.lockObject.Dispose();
            }
            this.alreadyDisposed = true;
        }

        public void Release()
        {
            this.Dispose();
        }
    }
}