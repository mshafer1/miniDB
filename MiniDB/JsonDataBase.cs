using MiniDB.Interfaces;

namespace MiniDB
{
    public class JsonDataBase<T> : DataBase where T :IDBObject
    {
        public JsonDataBase(string filename, float version, float minimumCompatibleVersion) : base(filename, version, minimumCompatibleVersion, new JsonStorageStrategy<T>(version, minimumCompatibleVersion))
        {
        }
    }
}
