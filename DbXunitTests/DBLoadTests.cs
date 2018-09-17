﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace DbXunitTests
{
    /// <summary>
    /// Test reloading databases
    /// </summary>
    public class DBLoadTests : IDisposable
    {
        /// <summary>
        /// db filename
        /// </summary>
        private readonly string filename;

        /// <summary>
        /// Second db filename
        /// </summary>
        private readonly string filename2;

        /// <summary>
        /// filename that transactions are stored in (dictated by DB).
        /// </summary>
        private readonly string transactionsFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBLoadTests" /> class.
        /// Setup up filenames and make sure the system is clean.
        /// </summary>
        public DBLoadTests()
        {
            this.filename = "TestDB_Loading.json";
            this.filename2 = "SecondDB_" + this.filename;
            this.transactionsFile = "transactions_" + this.filename + ".data";

            // make sure it is clean here as we start
            this.Cleanup();
        }

        /// <summary>
        /// In beteen each test, cleanup.
        /// </summary>
        public void Dispose()
        {
            // clean up when finished
            this.Cleanup();
        }

        /// <summary>
        /// Test that a base DB can reload correctly several times
        /// </summary>
        [Fact]
        public void TestNormalDBCanReload()
        {
            Console.WriteLine($"Test reloading normal DB");
            this.TestDBType(file => new MiniDB.DataBase<ExampleComplicatedStoredItem>(file, 1.0f, 1)); // TODO: add storage strategy
        }

        /// <summary>
        /// Test that even with encryption, a DB can be cached an reloaded multiple times
        /// </summary>
        [Fact]
        public void TestEncryptedDBCanReload()
        {
            Console.WriteLine($"Test reloading encrypted DB");
            this.TestDBType(file => new MiniDB.EncryptedDataBase<ExampleComplicatedStoredItem>(file, 1, 1)); // TODO: add storage strategy
        }

        /// <summary>
        /// Test that re-opening a db of the same type in the same file fails (mutex should prevent it).
        /// </summary>
        [Fact]
        public void TestCannotReloadSameDBTypeWithSameFile()
        {
            Console.WriteLine($"Test Cannot reload same DB type with same file");
            using (var db = new MiniDB.DataBase<ExampleComplicatedStoredItem>(this.filename, 1, 1))
            {
                Assert.Throws<MiniDB.DBCreationException>(() => new MiniDB.DataBase<ExampleComplicatedStoredItem>(this.filename, 1, 1));
            }
        }

        /// <summary>
        /// Test that a db can have two copies initialized if they use two different files
        /// </summary>
        [Fact]
        public void TestCanReloadSameDBTypeWithDifferentFile()
        {
            Console.WriteLine($"Test Can reload same DB type with different file");
            using (var db = new MiniDB.DataBase<ExampleComplicatedStoredItem>(this.filename, 1, 1))
            {
                new MiniDB.DataBase<ExampleComplicatedStoredItem>(this.filename2, 1, 1); // should not throw
                Assert.True(true); // if it made it this far, test is a success.
            }
        }

        /// <summary>
        /// Test that db is disposed and can be reloaded
        /// </summary>
        [Fact]
        public void TestCanUseUsingToReloadSameDBTypeWithSameFile()
        {
            using (var db = new MiniDB.DataBase<ExampleComplicatedStoredItem>(this.filename, 1, 1))
            {
                // NO-OP
            }

            using (var db2 = new MiniDB.DataBase<ExampleComplicatedStoredItem>(this.filename, 1, 1))
            {
                // create second DB of same type after cleaning the last one - this should succeed
            }

            Assert.True(true); // if it made it this far, test is a success.
        }

        /// <summary>
        /// Helper method to test reloading different DB class types
        /// </summary>
        /// <param name="createDB">function that creates a db (encrypted or not) of the ExampleComplicatedStoredItems at the specified file path.</param>
        private void TestDBType(Func<string, MiniDB.DataBase<ExampleComplicatedStoredItem>> createDB)
        {
            MiniDB.ID id;
            Debug.WriteLine($"Successfully reloaded 0 times");
            using (var db = createDB(this.filename))
            {
                var entry = new ExampleComplicatedStoredItem("John", "Doe");
                id = entry.ID;
                entry.Age = 0;
                db.Add(entry);
                entry.Age = 1;
            }

            Debug.WriteLine($"Successfully created");
            for (int i = 1; i <= 10; i++)
            {
                using (var db = createDB(this.filename))
                {
                    Debug.WriteLine($"Successfully reloaded {i} times");
                    Assert.Single(db);
                    var entry = db.FirstOrDefault();
                    Assert.Equal(entry.ID, id);
                    Assert.Equal(i, entry.Age);
                    Assert.True(db.CanUndo);
                    Assert.False(db.CanRedo);
                    db.Undo();
                    Assert.Single(db);
                    Assert.Equal(i - 1, entry.Age);
                    db.Redo();
                    entry.Age = i + 1; // trigger re-save
                }
            }
        }

        #region helper methods

        #region cleanup
        /// <summary>
        /// remove files that represent the database and the transactions file
        /// </summary>
        private void Cleanup()
        {
            var filesToDelete = new List<string>()
            {
                this.filename,
                this.filename2,
                this.transactionsFile
            };
            filesToDelete.Where(File.Exists).ToList().ForEach(File.Delete);
        }

        /// <summary>
        /// After all tests have run, clear resources
        /// </summary>
        private void Finally()
        {
            this.Dispose();
        }
        #endregion
        #endregion
    }
}
