using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MiniDB;
using MiniDB.Interfaces;

using Xunit;

namespace DbXunitTests.SystemTests
{
    public class UndoRedoWithJsonStorageStrategy : IDisposable
    {
        /// <summary>
        /// db filename
        /// </summary>
        private readonly string filename;

        /// <summary>
        /// filename that transactions are stored in (dictated by DB).
        /// </summary>
        private readonly string transactionsFile;

        public UndoRedoWithJsonStorageStrategy()
        {
            this.filename = "TestDB_Loading.json";
            this.transactionsFile = "transactions_" + this.filename + ".data";

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

        [Fact]
        public void Test_AddChangeItemUndo()
        {
            // Arrange
            var entry = new ExampleStoredItem("John", "Doe");
            var old_age = entry.Age = 0;
            var db = new MiniDB.JsonDataBase<ExampleStoredItem>(this.filename, 1.0f, 1.0f);

            db.Add(entry);
            entry.Age = 5;

            // Act
            db.Undo();

            // Assert
            Assert.Equal(old_age, ((ExampleStoredItem)db.First()).Age);
            Assert.True(db.CanUndo, "Should be able to Undo an edit to a DB item");
            Assert.True(db.CanRedo, "Just Undid!");
        }

        /// <summary>
        /// remove files that represent the database and the transactions files
        /// </summary>
        private void Cleanup()
        {
            var filesToDelete = new string[] { this.filename, this.transactionsFile };

            foreach (var file in filesToDelete)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }
    }
}
