using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace DbXunitTests
{
    public class DBLoadTests : IDisposable
    {
        private readonly string Filename;
        private readonly string Filename2;
        private readonly string transactionsFile;

        public DBLoadTests()
        {
            this.Filename = "TestDB.json";
            this.Filename2 = "SecondDB_" + this.Filename;
            transactionsFile = "transactions_" + this.Filename + ".data";

            // make sure it is clean here as we start
            Cleanup();
        }

        public void Dispose()
        {
            // clean up when finished
            Cleanup();
        }

        private void Cleanup()
        {
            if (File.Exists(this.Filename))
            {
                File.Delete(this.Filename);
            }

            if (File.Exists(this.Filename2))
            {
                File.Delete(this.Filename2);
            }

            if (File.Exists(transactionsFile))
            {
                File.Delete(transactionsFile);
            }
        }

        private void Finally()
        {
            this.Dispose();
        }

        [Fact]
        public void TestNormalDBCanReload()
        {
            TestDBType(file => new MiniDB.DataBase<ExampleComplicatedStoredItem>(file, 1.0f, 1));
        }

        [Fact]
        public void TestEncryptedDBCanReload()
        {
            TestDBType(file => new MiniDB.EncryptedDataBase<ExampleComplicatedStoredItem>(file, 1, 1));
        }

        [Fact]
        public void TestCannotReloadSameDBTypeWithSameFile()
        {
            using (var db = new MiniDB.DataBase<ExampleComplicatedStoredItem>(this.Filename, 1, 1))
            {
                Assert.Throws<MiniDB.DBCreationException>(() => new MiniDB.DataBase<ExampleComplicatedStoredItem>(this.Filename, 1, 1));
            }
        }

        [Fact]
        public void TestCanReloadSameDBTypeWithDifferentFile()
        {
            using (var db = new MiniDB.DataBase<ExampleComplicatedStoredItem>(this.Filename, 1, 1))
            {
                new MiniDB.DataBase<ExampleComplicatedStoredItem>(this.Filename2, 1, 1); // should not throw
                Assert.True(true); // if it made it this far, test is a success.
            }

        }

        [Fact]
        public void TestCanUseUsingToReloadSameDBTypeWithSameFile()
        {
            using (var db = new MiniDB.DataBase<ExampleComplicatedStoredItem>(this.Filename, 1, 1))
            {
                // NO-OP
            }
            using (var db2 = new MiniDB.DataBase<ExampleComplicatedStoredItem>(this.Filename, 1, 1))
            {
                // create second DB of same type after cleaning the last one - this should succeed
            }

            Assert.True(true); // if it made it this far, test is a success.
        }

        private void TestDBType(Func<string, MiniDB.DataBase<ExampleComplicatedStoredItem>> createDB)
        {
            MiniDB.ID id;
            Console.WriteLine($"Successfully reloaded 0 times");
            Debug.WriteLine($"Successfully reloaded 0 times");
            using (var db = createDB(this.Filename))
            {
                var entry = new ExampleComplicatedStoredItem("John", "Doe");
                id = entry.ID;
                entry.Age = 0;
                //camper.Age = 1;
                db.Add(entry);
                entry.Age = 1;
            }
            Console.WriteLine($"Successfully created");
            Debug.WriteLine($"Successfully created");
            for (int i = 1; i <= 10; i++)
            {
                using (var db = createDB(this.Filename))
                {
                    Debug.WriteLine($"Successfully reloaded {i} times");
                    Console.WriteLine($"Successfully reloaded {i} times");
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
    }
}
