using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDB
{
    public class JsonDataBase : DataBase
    {
        public JsonDataBase(string filename, float version, float minimumCompatibleVersion) : base(filename, version, minimumCompatibleVersion, new JsonStorageStrategy(version, minimumCompatibleVersion))
        {
        }
    }
}
