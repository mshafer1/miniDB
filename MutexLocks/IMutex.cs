namespace MutexLocks
{
    public interface IMutex
    {
        void Lock(string name);
        void Unlock();
        MutexObject Get();
    }
}
