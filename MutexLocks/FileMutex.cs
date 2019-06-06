using System;
using System.IO;

namespace MutexLocks
{
    public class FileMutex : IDisposable, IMutex
    {
        private readonly string file_name;
        private FileStream file;

        public FileMutex (string lock_name)
        {
            this.file_name = lock_name.Replace(':', '_').Replace(Path.DirectorySeparatorChar, '.').Trim('.') + ".lock";
        }

        public void Dispose()
        {
            this.Unlock();
        }

        public MutexObject Get()
        {
            try
            {
                this.file = File.Open(this.file_name, FileMode.OpenOrCreate);
            }
            catch (System.IO.IOException)
            {
                throw new MutexException($"Cannot open file {this.file_name}, another process is using it.");
            }

            return new MutexObject(this);
        }

        public void Unlock()
        {
            if (this.file == null)
            {
                return;
            }
            this.file.Close();
            this.file = null;

            File.Delete(this.file_name);
        }
    }
}
