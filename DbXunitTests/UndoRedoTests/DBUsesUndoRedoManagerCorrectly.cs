using MiniDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace DbXunitTests.UndoRedoTests
{
    public class DBUsesUndoRedoManagerCorrectly : IDisposable
    {
        private readonly MiniDB.Interfaces.StubUndoRedoManager manager;
        private readonly DataBase testDB;
        private readonly NullWriterStorageStrategy storageStrategy;

        public DBUsesUndoRedoManagerCorrectly()
        {
            this.manager = new MiniDB.Interfaces.StubUndoRedoManager();
            this.storageStrategy = new NullWriterStorageStrategy();

            this.testDB = new MiniDB.DataBase("test", 1, 1, this.storageStrategy, this.manager);
        }

        public void Dispose()
        {
            // NOOP
            this.testDB.Dispose();
        }

        [Fact]
        public void Test_DBCanUndoMirrorsManager()
        {
            this.manager.CheckCanUndo = (dontCare) => { return false; };

            Assert.False(this.testDB.CanUndo);

            this.manager.CheckCanUndo = (dontCare) => { return true; };

            Assert.True(this.testDB.CanUndo);
        }

        [Fact]
        public void Test_DBCanRedoMirrorsManager()
        {
            this.manager.CheckCanRedo = (dontCare) => { return false; };

            Assert.False(this.testDB.CanRedo);

            this.manager.CheckCanRedo = (dontCare) => { return true; };

            Assert.True(this.testDB.CanRedo);
        }

        [Fact]
        public void Test_DBUndoThrowsIfCannotUndo()
        {
            this.manager.CheckCanUndo = (dontCare) => { return false; };

            Assert.ThrowsAny<DBException>(() => 
            {
                this.testDB.Undo();
            });
        }

        [Fact]
        public void Test_DBUndoThrowsIfCannotRedo()
        {
            this.manager.CheckCanRedo = (dontCare) => { return false; };

            Assert.ThrowsAny<DBException>(() =>
            {
                this.testDB.Redo();
            });
        }

        [Fact]
        public void Test_DBUndoCallsManagerAndStoresUndoTransaction()
        {
            this.manager.CheckCanUndo = (dontCare) => { return true; };
            this.storageStrategy.ClearWroteFlags();

            var result = new MiniDB.Transactions.UndoTransaction();

            bool wroteToTransactions = false;
            this.storageStrategy.WroteTransactions += (data) => 
            {
                var insertedTransaction = data.Last();
                Assert.True(ReferenceEquals(insertedTransaction, result));
                wroteToTransactions = true;
            };

            this.testDB.Undo();

            // assert no-throw
            // assert that `result` is what is added to transactions in call back
            // assert callback called
            Assert.True(wroteToTransactions);
        }

        [Fact]
        public void Test_DBRedoCallsManagerAndStoresRedoTransaction()
        {
            this.manager.CheckCanRedo = (dontCare) => { return true; };
            this.storageStrategy.ClearWroteFlags();

            var result = new MiniDB.Transactions.UndoTransaction();


            bool wroteToTransactions = false;

            this.storageStrategy.WroteTransactions += (data) =>
            {
                var insertedTransaction = data.Last();
                Assert.True(ReferenceEquals(insertedTransaction, result));
                wroteToTransactions = true;
            };

            this.testDB.Redo();

            // assert no-throw
            // assert that `result` is what is added to transactions in call back
            // assert callback called
            Assert.True(wroteToTransactions);
        }
    }
}
