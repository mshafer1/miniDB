using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    public static class DBStorageStrategies<T> where T : DatabaseObject
    {
        public static IStorageStrategy<T> JSON {get {return JsonStorageStrategy<T>.Instance;} }
        public static IStorageStrategy<T> Encrypted { get { return EncryptedStorageStrategy<T>.Instance; } }
    }
}
