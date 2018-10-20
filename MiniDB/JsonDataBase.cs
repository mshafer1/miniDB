using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    public class JsonDataBase<T> : DataBase where T :IDatabaseObject
    {
        public JsonDataBase(string filename, float version, float minimumCompatibleVersion) : base(filename, version, minimumCompatibleVersion, new JsonStorageStrategy<T>(version, minimumCompatibleVersion))
        {
        }
    }
}
