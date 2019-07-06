using System;
using System.IO;

namespace MutexLocks
{
    public class FileMutex : IDisposable, IMutex
    {
        private readonly string file_name;
        private FileStream file;

        public FileMutex(string lock_name)
        {
            var dir = Path.GetDirectoryName(lock_name);
            var file = Path.GetFileName(lock_name) + ".lock";
            this.file_name = Path.Combine(dir, file);
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
            catch (IOException)
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
