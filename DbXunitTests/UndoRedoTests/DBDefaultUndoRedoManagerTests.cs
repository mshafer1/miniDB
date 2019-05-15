using MiniDB;
using MiniDB.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DbXunitTests.UndoRedoTests
{
    public class DBDefaultUndoRedoManagerTests : IDisposable
    {
        private readonly IUndoRedoManager manager;
        private readonly DataBase testDB;
        private readonly NullWriterStorageStrategy storageStrategy;

        public DBDefaultUndoRedoManagerTests()
        {
            this.manager = new MiniDB.Transactions.UndoRedoManager();
            this.storageStrategy = new NullWriterStorageStrategy();

            this.testDB = new MiniDB.DataBase("test", 1, 1, this.storageStrategy, this.manager);
        }

        public void Dispose()
        {
            // NOOP
            this.testDB.Dispose();
        }

        [Fact]
        public void Test_UndoRdeoAdd()
        {
            // Arrange
            Assert.False(this.testDB.CanUndo);
            this.storageStrategy.ClearWroteFlags();

            var entry = new ExampleStoredItem("John", "Doe");
            this.testDB.Add(entry);

            // add calls storage strategy is tested in storage strategy tests
            Assert.True(this.testDB.CanUndo, "Should be able to undo an add to empty db");
            Assert.False(this.testDB.CanRedo, "Should not be able to redo an add to an empty db");
            this.storageStrategy.ClearWroteFlags();

            // Act
            this.testDB.Undo();

            // Assert
            Assert.Empty(this.testDB);
            Assert.False(this.testDB.CanUndo, "DB should be empty again and not undoable");
            Assert.True(this.testDB.CanRedo, "Should be able to redo an undo");
            Assert.True(this.storageStrategy.WroteFlag, "Should have written to the db file");
            Assert.True(this.storageStrategy.WroteTransactionsFlag, "Should have written to the transactions file");

            // Rea-add
            this.testDB.Redo();

            // Assert
            Assert.Single(this.testDB);
            Assert.False(this.testDB.CanRedo);
            Assert.True(this.testDB.CanUndo);
        }
    }
}
