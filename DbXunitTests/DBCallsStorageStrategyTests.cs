using System;

using Xunit;

namespace DbXunitTests
{
    public class DBCallsStorageStrategyTests : IDisposable
    {
        private readonly MiniDB.DataBase testDB;
        private readonly NullWriterStorageStrategy nullWritingStorageStrategy;

        public DBCallsStorageStrategyTests()
        {
            this.nullWritingStorageStrategy = new NullWriterStorageStrategy();
            this.testDB = new MiniDB.DataBase(new MiniDB.DBMetadata("testDB.json", 1, 1), this.nullWritingStorageStrategy);
        }

        public void Dispose()
        {
            this.testDB?.Dispose();
        }

        [Fact]
        public void DoesNotWriteMainFileOnCreate()
        {
            Assert.False(this.nullWritingStorageStrategy.WroteFlag);
        }

        [Fact]
        public void DoesNotWriteTransactionsFileOnCreate()
        {
            Assert.False(this.nullWritingStorageStrategy.WroteTransactionsFlag);
        }

        [Fact]
        public void DoesWriteMainFileOnInsert()
        {
            Assert.False(this.nullWritingStorageStrategy.WroteFlag);
            this.testDB.Add(new ExampleStoredItem());
            Assert.True(this.nullWritingStorageStrategy.WroteFlag);
        }

        [Fact]
        public void DoesWriteTransactionsFileOnInsert()
        {
            this.nullWritingStorageStrategy.ClearWroteFlags();
            Assert.False(this.nullWritingStorageStrategy.WroteTransactionsFlag);

            this.testDB.Add(new ExampleStoredItem());

            Assert.True(this.nullWritingStorageStrategy.WroteTransactionsFlag);
        }

        [Fact]
        public void DoesWriteMainFileOnDelete()
        {
            var item = new ExampleStoredItem();
            this.testDB.Add(item);
            this.nullWritingStorageStrategy.ClearWroteFlags();
            Assert.False(this.nullWritingStorageStrategy.WroteFlag);

            this.testDB.Remove(item);
            Assert.True(this.nullWritingStorageStrategy.WroteFlag);
        }

        [Fact]
        public void DoesWriteTransactionsFileOnDelete()
        {
            var item = new ExampleStoredItem();
            this.testDB.Add(item);
            this.nullWritingStorageStrategy.ClearWroteFlags();
            Assert.False(this.nullWritingStorageStrategy.WroteTransactionsFlag);

            this.testDB.Remove(item);

            Assert.True(this.nullWritingStorageStrategy.WroteTransactionsFlag);
        }

        [Fact]
        public void DoesWriteMainFileOnItemEdit()
        {
            var item = new ExampleStoredItem("John", "Dow");
            this.testDB.Add(item);
            this.nullWritingStorageStrategy.ClearWroteFlags();
            Assert.False(this.nullWritingStorageStrategy.WroteFlag);

            item.FirstName = "Jane";
            Assert.True(this.nullWritingStorageStrategy.WroteFlag);
        }

        // TODO: DoesWriteTransactionsFileOnEditItem
        [Fact]
        public void DoesWriteMainFileOnItemNestedEdit()
        {
            var item = new ExampleComplicatedStoredItem("John", "Doe");
            item.Address.FirstLine = "P.O. Box";
            this.testDB.Add(item);
            this.nullWritingStorageStrategy.ClearWroteFlags();
            Assert.False(this.nullWritingStorageStrategy.WroteFlag);

            item.Address.FirstLine = "PO Box";
            Assert.True(this.nullWritingStorageStrategy.WroteFlag);
        }

        // TODO: DoesWriteTransactionsFileOnEditItemNested
    }
}
