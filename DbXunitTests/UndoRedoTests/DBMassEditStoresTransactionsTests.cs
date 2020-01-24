using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace DbXunitTests.UndoRedoTests
{
    public class DBMassEditStoresTransactionsTests : IDisposable
    {
        #region Fields
        private readonly MiniDB.DataBase testDB;
        private readonly NullWriterStorageStrategy nullWritingStorageStrategy;
        #endregion

        #region Constructors
        public DBMassEditStoresTransactionsTests()
        {
            var dbFilename = "testDB.json"; // this is ignored because the nullStorageStrategy doesn't care, but let's be consistent.

            this.nullWritingStorageStrategy = new NullWriterStorageStrategy();
            this.testDB = new MiniDB.DataBase(new MiniDB.DBMetadata(dbFilename, 1, 1), this.nullWritingStorageStrategy);
        }
        #endregion

        #region dispose/destruct
        ~DBMassEditStoresTransactionsTests()
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

        #region Tests
        [Fact]
        public void Test_MassRemoveLogsDeletes_Clear()
        {
            var thingsToAdd = new List<MiniDB.Interfaces.IDBObject>()
            {
                new ExampleStoredItem("John", "Doe"),
                new ExampleStoredItem("Jane", "Doe"),
                new ExampleStoredItem("Molly", "Doe"),
            };

            int remove_count = 0;

            this.nullWritingStorageStrategy.WroteTransactions += (data) =>
            {
                var lastTransaction = data.First();
                if (lastTransaction.DBTransactionType == MiniDB.DBTransactionType.Delete)
                {
                    ++remove_count;
                }
            };

            foreach (var item in thingsToAdd)
            {
                this.testDB.Add(item);
            }

            Assert.True(remove_count == 0, "Nothing should be removed yet");

            this.testDB.Clear();

            Assert.Equal(thingsToAdd.Count, remove_count);
        }

        [Fact]
        public void Test_RemoveAtLogsDeletes()
        {
            var thingsToAdd = new List<MiniDB.Interfaces.IDBObject>()
            {
                new ExampleStoredItem("John", "Doe"),
                new ExampleStoredItem("Jane", "Doe"),
                new ExampleStoredItem("Molly", "Doe"),
            };

            int remove_count = 0;

            this.nullWritingStorageStrategy.WroteTransactions += (data) =>
            {
                var lastTransaction = data.First();
                if (lastTransaction.DBTransactionType == MiniDB.DBTransactionType.Delete)
                {
                    ++remove_count;
                }
            };

            foreach (var item in thingsToAdd)
            {
                this.testDB.Add(item);
            }

            Assert.True(remove_count == 0, "Nothing should be removed yet");

            this.testDB.RemoveAt(1);

            Assert.Equal(1, remove_count);
        }

        [Fact]
        public void Test_SwapLogsRemoveThenAdd()
        {
            var thingsToAdd = new List<MiniDB.Interfaces.IDBObject>()
            {
                new ExampleStoredItem("John", "Doe"),
                new ExampleStoredItem("Jane", "Doe"),
                new ExampleStoredItem("Molly", "Doe"),
            };

            foreach (var item in thingsToAdd)
            {
                this.testDB.Add(item);
            }

            int remove_count = 0;
            int add_count = 0;
            string actionOrder = string.Empty;
            this.nullWritingStorageStrategy.WroteTransactions += (data) =>
            {
                var lastTransaction = data.First();
                if (lastTransaction.DBTransactionType == MiniDB.DBTransactionType.Delete)
                {
                    ++remove_count;
                    actionOrder += "r";
                }
                else if (lastTransaction.DBTransactionType == MiniDB.DBTransactionType.Add)
                {
                    ++add_count;
                    actionOrder += "a";
                }
            };

            var jsmith = new ExampleStoredItem("John", "Smith");
            Assert.True(remove_count == 0, "Nothing should be removed yet");

            this.testDB[1] = jsmith;

            Assert.Equal(1, remove_count);
            Assert.Equal(1, add_count);
            Assert.True(actionOrder == "ra", "Error, expected a remove, then an add . . .");
        }

        #endregion

        #region cleanup

        /// <summary>
        /// remove files that represent the database and the transactions files
        /// </summary>
        private void Cleanup()
        {
            // if using NullWriter, no files to clean up, but clear the DB
            this.testDB.Dispose();
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
