namespace MutexLocks
{
    public interface IMutex
    {
        void Unlock();
        MutexObject Get();
    }
}
