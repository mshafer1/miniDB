using System;
using System.Linq;
using Xunit;

using MiniDB;
using MiniDB.Interfaces;

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
            this.testDB.Dispose();
        }

        [Fact]
        public void Test_Setup()
        {
            // Arrange
            var db = new DBStateBuilder(this.testDB).Get_DB();

            // Assert
            Assert.False(db.CanUndo, "Should not be able to Undo an empty DB");
            Assert.False(db.CanRedo, "Should not be able to Redo an empty DB");
        }

        [Fact]
        public void Test_Add()
        {
            // Arrange
            this.storageStrategy.ClearWroteFlags();
            var entry = new ExampleStoredItem("John", "Doe");
            var db = new DBStateBuilder(this.testDB)
                .Get_DB();

            // Act
            db.Add(entry);

            // Assert
            Assert.True(db.CanUndo, "Should be able to Undo an add to a DB");
            Assert.False(db.CanRedo, "Should not be able to Redo without undoing");
        }

        [Fact]
        public void Test_AddUndo()
        {
            // Arrange
            var entry = new ExampleStoredItem("John", "Doe");
            var db = new DBStateBuilder(this.testDB)
                .AddItem(entry)
                .Get_DB();
            this.storageStrategy.ClearWroteFlags();

            // Act
            db.Undo();

            // Assert
            Assert.Empty(this.testDB);
            Assert.False(this.testDB.CanUndo, "DB should be empty again and not undoable");
            Assert.True(this.testDB.CanRedo, "Should be able to redo an undo");
            this.AssertStorageCached();
        }

        [Fact]
        public void Test_AddUndoRedo()
        {
            // Arrange
            var entry = new ExampleStoredItem("John", "Doe");
            var db = new DBStateBuilder(this.testDB)
                .AddItem(entry)
                .Undo()
                .Get_DB();
            this.storageStrategy.ClearWroteFlags();

            // Act
            db.Redo();

            // Assert
            Assert.Single(this.testDB);
            Assert.False(this.testDB.CanRedo);
            Assert.True(this.testDB.CanUndo);

            var item = this.testDB.First();
            Assert.True(item == entry);
            this.AssertStorageCached();
        }

        [Fact]
        public void Test_AddUndoRedoUndo()
        {
            // Arrange
            var entry = new ExampleStoredItem("John", "Doe");
            var db = new DBStateBuilder(this.testDB)
                .AddItem(entry)
                .Undo()
                .Redo()
                .Get_DB();
            this.storageStrategy.ClearWroteFlags();

            // Act
            db.Undo();

            // Assert
            Assert.Empty(this.testDB);
            Assert.True(this.testDB.CanRedo, "Just undid again!");
            Assert.False(this.testDB.CanUndo, "should be NOT able to undo on empty");
            this.AssertStorageCached();
        }

        [Fact]
        public void Test_AddUndoRedoUndoRedo()
        {
            // Arrange
            var entry = new ExampleStoredItem("John", "Doe");
            var db = new DBStateBuilder(this.testDB)
                .AddItem(entry)
                .Undo()
                .Redo()
                .Undo()
                .Get_DB();
            this.storageStrategy.ClearWroteFlags();

            // Act
            db.Redo();

            // Assert
            Assert.Single(this.testDB);
            Assert.False(this.testDB.CanRedo, "Should be back to the top of the edit stack");
            Assert.True(this.testDB.CanUndo, "Just redid!");
            this.AssertStorageCached();
        }

        [Fact]
        public void Test_AddUndoRedoUndoRedoUndo()
        {
            // Arrange
            var entry = new ExampleStoredItem("John", "Doe");
            var db = new DBStateBuilder(this.testDB)
                .AddItem(entry)
                .Undo()
                .Redo()
                .Undo()
                .Redo()
                .Get_DB();
            this.storageStrategy.ClearWroteFlags();

            // Act
            db.Undo();

            // Assert
            Assert.Empty(this.testDB);
            Assert.True(this.testDB.CanRedo, "Just undid again!");
            Assert.False(this.testDB.CanUndo, "should be NOT able to undo on empty");
            this.AssertStorageCached();
        }


        [Fact]
        public void Test_AddItem()
        {
            // Arrange
            this.storageStrategy.ClearWroteFlags();
            var entry = new ExampleStoredItem("John", "Doe");
            var db = new DBStateBuilder(this.testDB)
                .AddItem(entry)
                .Get_DB();
            var item = (ExampleStoredItem)db.First();

            // Assert
            Assert.Equal(entry, item);
            Assert.True(entry == item, "Device comparison should also work");
            Assert.True(db.CanUndo, "Should be able to Undo an add to a DB");
            Assert.False(db.CanRedo, "Should not be able to Redo without undoing");
            this.AssertStorageCached();
        }

        [Fact]
        public void Test_AddChangeItem()
        {
            // Arrange
            var entry = new ExampleStoredItem("John", "Doe");
            var db = new DBStateBuilder(this.testDB)
                .AddItem(entry)
                .Get_DB();
            var item = (ExampleStoredItem)db.First();
            this.storageStrategy.ClearWroteFlags();

            // Act
            item.Age++;

            // Assert
            Assert.Equal(entry, item);
            Assert.True(entry == item, "Device comparison should also work");
            Assert.True(db.CanUndo, "Should be able to Undo an edit to a DB item");
            Assert.False(db.CanRedo, "Should not be able to Redo without undoing");
            this.AssertStorageCached();
        }

        [Fact]
        public void Test_AddChangeItemUndo()
        {
            // Arrange
            var entry = new ExampleStoredItem("John", "Doe");
            var old_age = entry.Age = 0;
            var db = new DBStateBuilder(this.testDB)
                .AddItem(entry)
                .EditItem(item => { ((ExampleStoredItem)item).Age++; })
                .Get_DB();
            this.storageStrategy.ClearWroteFlags();

            // Act
            db.Undo();

            // Assert
            Assert.Equal(old_age, ((ExampleStoredItem)db.First()).Age);
            Assert.True(db.CanUndo, "Should be able to Undo an edit to a DB item");
            Assert.True(db.CanRedo, "Just Undid!");
            this.AssertStorageCached();
        }

        [Fact]
        public void Test_AddChangeItemUndoUndo()
        {
            // Arrange
            var entry = new ExampleStoredItem("John", "Doe");
            var old_age = entry.Age = 0;
            var db = new DBStateBuilder(this.testDB)
                .AddItem(entry)
                .EditItem(item => { ((ExampleStoredItem)item).Age++; })
                .Undo()
                .Get_DB();
            this.storageStrategy.ClearWroteFlags();

            // Act
            db.Undo();

            // Assert
            Assert.Empty(db);
            Assert.False(db.CanUndo, "Should be empty again!");
            Assert.True(db.CanRedo, "Just Undid!");
            this.AssertStorageCached();
        }

        [Fact]
        public void Test_AddChangeItemUndoRedo()
        {
            // Arrange
            var entry = new ExampleStoredItem("John", "Doe");
            var old_age = entry.Age = 0;
            var db = new DBStateBuilder(this.testDB)
                .AddItem(entry)
                .EditItem(item => { ((ExampleStoredItem)item).Age++; })
                .Undo()
                .Get_DB();
            this.storageStrategy.ClearWroteFlags();

            // Act
            db.Redo();

            // Assert
            Assert.Equal(old_age + 1, ((ExampleStoredItem)db.First()).Age);
            Assert.True(db.CanUndo, "Should be able to Undo an edit to a DB item");
            Assert.False(db.CanRedo, "Should be at the top of the stack");
            this.AssertStorageCached();
        }

        [Fact]
        public void Test_AddChangeItemUndoRedoUndo()
        {
            // Arrange
            var entry = new ExampleStoredItem("John", "Doe");
            var old_age = entry.Age = 0;
            var db = new DBStateBuilder(this.testDB)
                .AddItem(entry)
                .EditItem(item => { ((ExampleStoredItem)item).Age++; })
                .Undo()
                .Redo()
                .Get_DB();
            this.storageStrategy.ClearWroteFlags();

            // Act
            db.Undo();

            // Assert
            Assert.Equal(old_age, ((ExampleStoredItem)db.First()).Age);
            Assert.True(db.CanUndo, "Should be able to Undo an edit to a DB item");
            Assert.True(db.CanRedo, "Just undid!");
            this.AssertStorageCached();
        }

        [Fact]
        public void Test_AddChangeItemUndoRedoUndoUndo()
        {
            // Arrange
            var entry = new ExampleStoredItem("John", "Doe");
            var old_age = entry.Age = 0;
            var db = new DBStateBuilder(this.testDB)
                .AddItem(entry)
                .EditItem(item => { ((ExampleStoredItem)item).Age++; })
                .Undo()
                .Redo()
                .Undo()
                .Get_DB();
            this.storageStrategy.ClearWroteFlags();

            // Act
            db.Undo();

            // Assert
            Assert.Empty(db);
            Assert.False(db.CanUndo, "Should be back to empty");
            Assert.True(db.CanRedo, "Just undid!");
            this.AssertStorageCached();
        }

        [Fact]
        public void Test_AddChangeItemUndoRedoUndoUndoRedoRedo()
        {
            // Arrange
            var entry = new ExampleStoredItem("John", "Doe");
            var old_age = entry.Age = 0;
            var db = new DBStateBuilder(this.testDB)
                .AddItem(entry)
                .EditItem(item => { ((ExampleStoredItem)item).Age++; })
                .Undo()
                .Redo()
                .Undo()
                .Undo()
                .Redo()
                .Get_DB();
            this.storageStrategy.ClearWroteFlags();

            // Act
            db.Redo();

            // Assert
            Assert.Equal(old_age + 1, ((ExampleStoredItem)db.First()).Age);
            Assert.True(db.CanUndo, "Should be able to Undo an edit to a DB item");
            Assert.False(db.CanRedo, "Should be at the top of the stack");
            this.AssertStorageCached();
        }

        [Fact]
        public void Test_AddEditUndo()
        {
            // Arrange
            var entry = new ExampleStoredItem("John", "Doe");
            var old_age = entry.Age = 0;
            var db = new DBStateBuilder(this.testDB)
                .AddItem(entry)
                .EditItem(item => { ((ExampleStoredItem)item).FirstName = "Zach"; })
                .Get_DB();
            this.storageStrategy.ClearWroteFlags();

            // Act
            db.Undo();

            // Assert
            Assert.Equal("John", entry.FirstName);
            Assert.True(db.CanUndo, "Should be able to Undo add");
            Assert.True(db.CanRedo, "Just Undid!");
            this.AssertStorageCached();
        }

        [Fact]
        public void Test_AddEditEditUndo()
        {
            // Arrange
            var entry = new ExampleStoredItem("John", "Doe");
            var old_age = entry.Age = 0;
            var db = new DBStateBuilder(this.testDB)
                .AddItem(entry)
                .EditItem(item => { ((ExampleStoredItem)item).FirstName = "Zach"; })
                .EditItem(item => { ((ExampleStoredItem)item).FirstName = "Zachary"; })
                .Get_DB();
            this.storageStrategy.ClearWroteFlags();

            // Act
            db.Undo();

            // Assert
            Assert.Equal("Zach", entry.FirstName);
            Assert.True(db.CanUndo, "Should be able to Undo an edit to a DB item");
            Assert.True(db.CanRedo, "Just Undid!");
            this.AssertStorageCached();
        }

        [Fact]
        public void Test_AddEditEditUndoEditUndo()
        {
            // Arrange
            var entry = new ExampleStoredItem("John", "Doe");
            var old_age = entry.Age = 0;
            var db = new DBStateBuilder(this.testDB)
                .AddItem(entry)
                .EditItem(item => { ((ExampleStoredItem)item).FirstName = "Zach"; })
                .EditItem(item => { ((ExampleStoredItem)item).FirstName = "Zachary"; })
                .Undo()
                .EditItem(item => { ((ExampleStoredItem)item).FirstName = "Zacha"; })
                .Get_DB();
            this.storageStrategy.ClearWroteFlags();

            // Act
            db.Undo();

            // Assert
            Assert.Equal("Zach", entry.FirstName);
            Assert.True(db.CanUndo, "Should be able to Undo add DB item");
            Assert.True(db.CanRedo, "Just Undid!");
            this.AssertStorageCached();
        }

        [Fact]
        public void Test_AddEditEditEditUndoUndo()
        {
            // Arrange
            var entry = new ExampleStoredItem("John", "Doe");
            var old_age = entry.Age = 0;
            var db = new DBStateBuilder(this.testDB)
                .AddItem(entry)
                .EditItem(item => { ((ExampleStoredItem)item).FirstName = "Zach"; })
                .EditItem(item => { ((ExampleStoredItem)item).FirstName = "Zachary"; })
                .EditItem(item => { ((ExampleStoredItem)item).FirstName = "Zacha"; })
                .Undo()
                .Get_DB();
            this.storageStrategy.ClearWroteFlags();

            // Act
            db.Undo();

            // Assert
            Assert.Equal("Zach", entry.FirstName);
            Assert.True(db.CanUndo, "Should be able to Undo an edit to a DB item");
            Assert.True(db.CanRedo, "Just Undid!");
            this.AssertStorageCached();
        }

        [Fact]
        public void Test_AddEditEditEditUndoUndoRedoUndo()
        {
            // Arrange
            var entry = new ExampleStoredItem("John", "Doe");
            var old_age = entry.Age = 0;
            var db = new DBStateBuilder(this.testDB)
                .AddItem(entry)
                .EditItem(item => { ((ExampleStoredItem)item).FirstName = "Zach"; })
                .EditItem(item => { ((ExampleStoredItem)item).FirstName = "Zachary"; })
                .EditItem(item => { ((ExampleStoredItem)item).FirstName = "Zacha"; })
                .Undo()
                .Undo()
                .Redo()
                .Get_DB();
            this.storageStrategy.ClearWroteFlags();

            // Act
            db.Undo();

            // Assert
            Assert.Equal("Zach", entry.FirstName);
            Assert.True(db.CanUndo, "Should be able to Undo an edit to a DB item");
            Assert.True(db.CanRedo, "Just Undid!");
            this.AssertStorageCached();
        }

        [Fact]
        public void Test_AddEditEditEditUndoUndoRedoUndoUndoUndo()
        {
            // Arrange
            var entry = new ExampleStoredItem("John", "Doe");
            var old_age = entry.Age = 0;
            var db = new DBStateBuilder(this.testDB)
                .AddItem(entry)
                .EditItem(item => { ((ExampleStoredItem)item).FirstName = "Zach"; })
                .EditItem(item => { ((ExampleStoredItem)item).FirstName = "Zachary"; })
                .EditItem(item => { ((ExampleStoredItem)item).FirstName = "Zacha"; })
                .Undo() // zachary
                .Undo() // zach
                .Redo() // zachary
                .Undo() // zach
                .Undo() // John
                .Get_DB();

            this.storageStrategy.ClearWroteFlags();

            // Act
            db.Undo(); // undo add

            // Assert
            Assert.Empty(db);
            Assert.False(db.CanUndo, "Should not be able to undo on empty db");
            Assert.True(db.CanRedo, "Just Undid!");
            this.AssertStorageCached();
        }

        private void AssertStorageCached()
        {
            Assert.True(this.storageStrategy.WroteFlag, "Should have written to the db file");
            Assert.True(this.storageStrategy.WroteTransactionsFlag, "Should have written to the transactions file");
        }
    }
}
