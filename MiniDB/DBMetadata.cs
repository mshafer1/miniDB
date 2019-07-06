using System.Collections.ObjectModel;
using System.IO;

using MiniDB.Interfaces;

namespace MiniDB
{
    public class DBMetadata : Collection<IDBObject>
    {
        public DBMetadata(string filename, float version, float minimumCompatibleVersion)
        {
            this.Filename = Path.GetFullPath(filename);
            this.DBVersion = version;
            this.MinimumCompatibleVersion = minimumCompatibleVersion;
        }

        public string Filename { get; }

        public float DBVersion { get; }

        public float MinimumCompatibleVersion { get; }
    }
}
