using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DbXunitTests
{
    public class DBCallsStorageStrategyTests : IDisposable
    {
        private MiniDB.DataBase testDB;
        private NullWriterStorageStrategy nullWritingStorageStrategy;

        public DBCallsStorageStrategyTests()
        {
            nullWritingStorageStrategy = new NullWriterStorageStrategy();
            testDB = new MiniDB.DataBase("testDB.json", 1, 1, nullWritingStorageStrategy);
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

        // TODO: DoesWriteTrancsactionOnInsert
        ////[Fact]
        ////public void DoesWriteTransactionsFileOnInsert()
        ////{
        ////    Assert.False(this.nullWritingStorageStrategy.WroteTransactionsFlag);
        ////    this.testDB.Add(new ExampleStoredItem());
        ////    Assert.True(this.nullWritingStorageStrategy.WroteTransactionsFlag);
        ////}

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

        // TODO: DoesWriteTransactionsFileOnDelete

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
