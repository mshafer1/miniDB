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
            this.testDB.Dispose();
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
            this.testDB.Add(new ExampleStoredItem());
            Assert.True(this.nullWritingStorageStrategy.WroteFlag);
        }

        [Fact]
        public void DoesWriteTransactionsFileOnInsert()
        {
            this.nullWritingStorageStrategy.ClearWroteFlags();

            this.testDB.Add(new ExampleStoredItem());

            Assert.True(this.nullWritingStorageStrategy.WroteTransactionsFlag);
        }

        [Fact]
        public void DoesWriteMainFileOnDelete()
        {
            var item = new ExampleStoredItem();
            this.testDB.Add(item);
            this.nullWritingStorageStrategy.ClearWroteFlags();

            this.testDB.Remove(item);

            Assert.True(this.nullWritingStorageStrategy.WroteFlag);
        }

        [Fact]
        public void DoesWriteTransactionsFileOnDelete()
        {
            var item = new ExampleStoredItem();
            this.testDB.Add(item);
            this.nullWritingStorageStrategy.ClearWroteFlags();

            this.testDB.Remove(item);

            Assert.True(this.nullWritingStorageStrategy.WroteTransactionsFlag);
        }

        [Fact]
        public void DoesWriteMainFileOnItemEdit()
        {
            var item = new ExampleStoredItem("John", "Dow");
            this.testDB.Add(item);
            this.nullWritingStorageStrategy.ClearWroteFlags();

            item.FirstName = "Jane";

            Assert.True(this.nullWritingStorageStrategy.WroteFlag);
        }

        [Fact]
        public void DoesWriteMainFileOnItemNestedEdit()
        {
            var item = new ExampleComplicatedStoredItem("John", "Doe");
            item.Address.FirstLine = "P.O. Box";
            this.testDB.Add(item);
            this.nullWritingStorageStrategy.ClearWroteFlags();

            item.Address.FirstLine = "PO Box";

            Assert.True(this.nullWritingStorageStrategy.WroteFlag);
        }
    }
}
