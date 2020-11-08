using System;
using Xunit;

namespace DbXunitTests.UndoRedoTests
{
    public class ExternalLogTransactionsCallStorageStrategyTests : IDisposable
    {
        private readonly MiniDB.DataBase testDB;
        private readonly NullWriterStorageStrategy nullWritingStorageStrategy;
        private readonly MiniDB.Transactions.UndoRedoManager undoRedoManager;

        public ExternalLogTransactionsCallStorageStrategyTests()
        {
            this.nullWritingStorageStrategy = new NullWriterStorageStrategy();
            this.undoRedoManager = new MiniDB.Transactions.UndoRedoManager(this.nullWritingStorageStrategy, "test.json");
            this.testDB = new MiniDB.DataBase(new MiniDB.DBMetadata("testDB.json", 1, 1), this.nullWritingStorageStrategy, this.undoRedoManager);
        }

        public void Dispose()
        {
            // NOOP
            this.testDB.Dispose();
        }

        [Fact]
        public void DoesWriteTransactionFileOnNestedDictionaryItemEdit()
        {
            var item = new ExampleComplicatedStoredItem("John", "Doe");
            var nestedItem = new AddressClass() { City = "Jonesboro" };
            var key = "Apple";
            item.OtherAddresses.Add(key, nestedItem);

            this.testDB.Add(item);
            this.testDB.RegisterNestedItem(item.ID, $"{nameof(item.OtherAddresses)}[{key}]");
            this.nullWritingStorageStrategy.ClearWroteFlags();

            item.OtherAddresses[key].City = "Test";

            Assert.True(this.nullWritingStorageStrategy.WroteFlag, "Should have written change down");
            Assert.True(this.nullWritingStorageStrategy.WroteTransactionsFlag, "Should have recorded the transaction");
        }

        [Fact]
        public void DoesWriteTransactionFileOnNestedDictionaryItemSubItemEdit()
        {
            var item = new ExampleComplicatedStoredItem("John", "Doe");
            var nestedItem = new AddressClass() { City = "Jonesboro" };
            var key = "Apple";
            item.OtherAddresses.Add(key, nestedItem);

            this.testDB.Add(item);
            this.testDB.RegisterNestedItem(item.ID, $"{nameof(item.OtherAddresses)}[{key}]");
            this.nullWritingStorageStrategy.ClearWroteFlags();

            item.OtherAddresses[key].Zip.Value = 12345;

            Assert.True(this.nullWritingStorageStrategy.WroteFlag, "Should have written change down");
            Assert.True(this.nullWritingStorageStrategy.WroteTransactionsFlag, "Should have recorded the transaction");
        }
    }
}
