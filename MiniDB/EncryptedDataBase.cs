using MiniDB.Interfaces;

namespace MiniDB
{
    public class EncryptedDataBase<T> : DataBase
        where T : IDBObject
    {
        public EncryptedDataBase(string filename, float version, float minimumCompatibleVersion)
            : base(filename, version, minimumCompatibleVersion, new EncryptedStorageStrategy<T>(version, minimumCompatibleVersion))
        {
        }
    }
}
