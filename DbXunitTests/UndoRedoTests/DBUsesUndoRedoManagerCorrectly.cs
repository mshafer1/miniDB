using MiniDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbXunitTests.UndoRedoTests
{
    class DBUsesUndoRedoManagerCorrectly : IDisposable
    {
        private MiniDB.Interfaces.IUndoRedoManager manager;
        private DataBase testDB;
        private IStorageStrategy storageStrategy;

        public DBUsesUndoRedoManagerCorrectly()
        {
            this.manager = new MiniDB.Interfaces.StubUndoRedoManager();
            this.storageStrategy = new NullWriterStorageStrategy();

            this.testDB = new MiniDB.DataBase("test", 1, 1, this.storageStrategy, this.manager);
        }

        public void Dispose()
        {
            manager = null;
        }
    }
}
