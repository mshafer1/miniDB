using System;
using System.IO;
using System.Threading;

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
                for(int i = 0; i < 3; i++)
                {
                    try
                    {
                        this.file = File.Open(this.file_name, FileMode.OpenOrCreate);

                        // if opening the file succeeds, return the MutexObject
                        return new MutexObject(this);
                    }
                    catch (IOException)
                    {
                        // if it fails, sleep for 1 second before trying again
                        Thread.Sleep(3 * 1000);
                    }
                }

                // one last try, if this one fails, the outer catch will raise
                this.file = File.Open(this.file_name, FileMode.OpenOrCreate);

                // if opening the file succeeds, return the MutexObject
                return new MutexObject(this);
            }
            catch (IOException)
            {
                throw new MutexException($"Cannot open file {this.file_name}, another process is using it.");
            }
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
