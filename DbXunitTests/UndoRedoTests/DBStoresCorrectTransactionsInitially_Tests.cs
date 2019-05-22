using System;
using System.Linq;

using Xunit;

namespace DbXunitTests.UndoRedoTests
{
    public class DBStoresCorrectTransactionsInitially_Tests : IDisposable
    {
        #region Fields
        private MiniDB.DataBase testDB;
        private NullWriterStorageStrategy nullWritingStorageStrategy;
        #endregion

        #region Constructors
        public DBStoresCorrectTransactionsInitially_Tests()
        {
            this.nullWritingStorageStrategy = new NullWriterStorageStrategy();
            this.testDB = new MiniDB.DataBase("testDB.json", 1, 1, this.nullWritingStorageStrategy);
        }
        #endregion

        #region dispose/destruct
        // cleanup once finished
        ~DBStoresCorrectTransactionsInitially_Tests()
        {
            this.Cleanup();
        }

        /// <summary>
        /// In between each test, cleanup.
        /// </summary>
        public void Dispose()
        {
            this.Cleanup();
        }
        #endregion

        [Fact]
        public void TestAddingItemToDBMakes_Add_Transaction_onInit()
        {
            bool addTransactionAdded = false;
            this.nullWritingStorageStrategy.WroteTransactions += (data) =>
            {
                var transaction = data.Last();
                addTransactionAdded = transaction.DBTransactionType == MiniDB.DBTransactionType.Add;
            };

            this.testDB.Add(new ExampleStoredItem("John", "Doe"));

            Assert.True(addTransactionAdded);
        }

        [Fact]
        public void TestChangingItemInDBMakes_Modify_Transaction_onInit()
        {
            bool modifyTransactionAdded = false;
            this.nullWritingStorageStrategy.WroteTransactions += (data) =>
            {
                var transaction = data.First();
                modifyTransactionAdded = transaction.DBTransactionType == MiniDB.DBTransactionType.Modify;
            };

            var jdoe = new ExampleStoredItem("John", "Doe");
            jdoe.Age = 10;

            this.testDB.Add(jdoe);
            Assert.False(modifyTransactionAdded, "Should not have added a modify yet");

            jdoe.Age = 11; // should trigger modify transaction

            Assert.True(modifyTransactionAdded);
        }

        [Fact]
        public void TestRemovingItemFromDBMakes_Delete_Transaction()
        {
            bool deleteTransactionAdded = false;
            this.nullWritingStorageStrategy.WroteTransactions += (data) =>
            {
                var transaction = data.First();
                deleteTransactionAdded = transaction.DBTransactionType == MiniDB.DBTransactionType.Delete;
            };

            var jdoe = new ExampleStoredItem("John", "Doe");

            this.testDB.Add(jdoe);
            Assert.False(deleteTransactionAdded, "Should not have added a delete yet");

            this.testDB.Remove(jdoe); // should trigger remove transaction

            Assert.True(deleteTransactionAdded);
        }

        #region cleanup
        /// <summary>
        /// remove files that represent the database and the transactions files
        /// </summary>
        private void Cleanup()
        {
            // if using NullWriter, no files to clean up, but clear the DB
            this.testDB?.Dispose();
        }

        /// <summary>
        /// After all tests have run, clear resources
        /// </summary>
        private void Finally()
        {
            this.Dispose();
        }
        #endregion
    }
}
