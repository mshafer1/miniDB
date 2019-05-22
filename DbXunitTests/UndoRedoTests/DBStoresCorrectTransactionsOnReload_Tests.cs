using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace DbXunitTests.UndoRedoTests
{
    public class DBStoresCorrectTransactionsOnReload_Tests : IDisposable
    {
        #region Fields
        private readonly MiniDB.IDBObject storedItem;
        private MiniDB.DataBase testDB;
        private NullWriterStorageStrategy nullWritingStorageStrategy;
        #endregion

        #region Constructors
        public DBStoresCorrectTransactionsOnReload_Tests()
        {
            var dbFilename = "testDB.json"; // this is ignored because the storageStrategy doesn't care, but let's be consistent.

            this.nullWritingStorageStrategy = new NullWriterStorageStrategy();

            // simulate reloading the DB by causing it to load with an item in it.
            var jdoe = new ExampleStoredItem("John", "Doe");
            jdoe.Age = 10;

            this.nullWritingStorageStrategy.dBObjects = new List<ExampleStoredItem>() { jdoe };

            this.testDB = new MiniDB.DataBase(dbFilename, 1, 1, this.nullWritingStorageStrategy);
            this.storedItem = this.testDB.FirstOrDefault(); // allow access for modifying it.

            if (this.storedItem == null)
            {
                throw new Exception("Failed to setup DB with item in it");
            }
        }
        #endregion

        #region dispose/destruct
        ~DBStoresCorrectTransactionsOnReload_Tests()
        {
            this.Cleanup();
        }

        /// <summary>
        /// In between each test, cleanup.
        /// </summary>
        public void Dispose()
        {
            // clean up when finished
            this.Cleanup();
        }
        #endregion

        [Fact]
        public void TestAddingItemToDBMakes_Add_Transaction()
        {
            bool addTransactionAdded = false;
            this.nullWritingStorageStrategy.WroteTransactions += (data) =>
            {
                var transaction = data.Last();
                addTransactionAdded = transaction.DBTransactionType == MiniDB.DBTransactionType.Add;
            };

            this.testDB.Add(new ExampleStoredItem("Jane", "Doe"));

            Assert.True(addTransactionAdded);
        }

        [Fact]
        public void TestChangingItemInDBMakes_Modify_Transaction_onExistingItem()
        {
            bool modifyTransactionAdded = false;
            this.nullWritingStorageStrategy.WroteTransactions += (data) =>
            {
                var transaction = data.Last();
                modifyTransactionAdded = transaction.DBTransactionType == MiniDB.DBTransactionType.Modify;
            };

            Assert.False(modifyTransactionAdded, "Should not have added a modify yet");

            var jdoe = this.storedItem as ExampleStoredItem; // using dynamic cast to keep original object reference.
            jdoe.Age = 11; // should trigger modify transaction

            Assert.True(modifyTransactionAdded);
        }

        [Fact]
        public void TestRemovingItemFromDBMakes_Delete_Transaction_onExistingItem()
        {
            bool ModifyTransactionAdded = false;
            this.nullWritingStorageStrategy.WroteTransactions += (data) =>
            {
                var transaction = data.Last();
                ModifyTransactionAdded = transaction.DBTransactionType == MiniDB.DBTransactionType.Delete;
            };

            this.testDB.Remove(this.storedItem);

            Assert.True(ModifyTransactionAdded);
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
