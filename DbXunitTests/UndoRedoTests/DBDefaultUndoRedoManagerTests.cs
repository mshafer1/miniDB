﻿using MiniDB;
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

        private void AssertStorageCached()
        {
            Assert.True(this.storageStrategy.WroteFlag, "Should have written to the db file");
            Assert.True(this.storageStrategy.WroteTransactionsFlag, "Should have written to the transactions file");
        }
    }
}
