using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB.DBStorageStrategies
{
    public class JsonDB<T> : DataBase<T> where T : IDatabaseObject
    {
        public JsonDB(string filename, float dbVersion, float minimumCompatibleVersion) : base(filename, dbVersion, minimumCompatibleVersion, new JsonStorageStrategy<T>())
        {
            // NOOP
        }
    }
}
