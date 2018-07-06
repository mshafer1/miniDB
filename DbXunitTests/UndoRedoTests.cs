using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

namespace DbXunitTests
{
    /// <summary>
    /// Test undo and redo operation on <see cref="DataBase"/> class.
    /// </summary>
    public class UndoRedoTests : IDisposable
    {
        /// <summary>
        /// the file to store the data in (pass to db)
        /// </summary>
        private readonly string filename;

        /// <summary>
        /// the file to store the transaction in (dictated by the db)
        /// </summary>
        private readonly string transactionsFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="UndoRedoTests"/> class.
        /// </summary>
        public UndoRedoTests()
        {
            this.filename = "TestDB_UndoRedo.json";
            this.transactionsFile = "transactions_" + this.filename + ".data";

            // make sure it is clean
            this.Cleanup();
        }

        /// <summary>
        /// Cleanup in between each test
        /// </summary>
        public void Dispose()
        {
            // clean up when finished
            this.Cleanup();
        }

        ~UndoRedoTests()
        {
            this.Cleanup();
        }

        #region test_methods

        /// <summary>
        /// Test undo and redo adding an item to the collection
        /// </summary>
        [Fact]
        public void DBModelUndoRedoAddTest()
        {
            Console.WriteLine("Test: DBModelUndoRedoAddTest");
            var entry = new ExampleStoredItem("John", "Doe");

            using (var db = new MiniDB.DataBase<ExampleStoredItem>(this.filename, 1, 1))
            {
                Assert.False(File.Exists(this.filename));
               this.Sleep(.300);
                db.Add(entry);
                Assert.True(File.Exists(this.filename));
                Assert.True(File.Exists(this.transactionsFile));
                Assert.Single(db);
                Assert.True(db.CanUndo);
                Assert.False(db.CanRedo);
                var edit_time = File.GetLastWriteTime(this.filename);

                this.Sleep(.300);
                db.Undo();
                Assert.True(File.Exists(this.filename));
                Assert.True(File.Exists(this.transactionsFile));
                Assert.Empty(db);
                Assert.NotEqual(edit_time, File.GetLastWriteTime(this.filename)); // should update on edit
                Assert.False(db.CanUndo);
                Assert.True(db.CanRedo);
                edit_time = File.GetLastWriteTime(this.filename);

                this.Sleep(.300);
                db.Redo();
                Assert.True(db.CanUndo);
                Assert.False(db.CanRedo);
                Assert.True(File.Exists(this.filename));
                Assert.True(File.Exists(this.transactionsFile));
                Assert.Single(db);
                Assert.NotEqual(edit_time, File.GetLastWriteTime(this.filename)); // should update on edit
                Assert.Equal("John", db.First().FirstName);
                Assert.Equal("Doe", db.First().LastName);
            }
        }

        /// <summary>
        /// Verify that changed properties are stored
        /// </summary>
        [Fact]
        public void DBModelRegistersAddedItemsForPropertyChangedEventsTest()
        {
            Console.WriteLine("Test: DBModelRegistersAddedItemsForPropertyChangedEventsTest");
            var entry = new ExampleStoredItem("John", "Doe");
            using (var db = new MiniDB.DataBase<ExampleStoredItem>(this.filename, 1, 1))
            {
                Assert.False(File.Exists(this.filename));

                this.Sleep(.300);
                db.Add(entry);
                Assert.True(File.Exists(this.filename));
                Assert.True(File.Exists(this.transactionsFile));
                var edit_time = File.GetLastWriteTime(this.filename);

                this.Sleep(.300);
                entry.FirstName = "John";
                Assert.Equal(edit_time, File.GetLastWriteTime(this.filename)); // should not update on no change

                this.Sleep(.300);
                entry.FirstName = "Johnny";
                Assert.NotEqual(edit_time, File.GetLastWriteTime(this.filename)); // should update on edit
                edit_time = File.GetLastWriteTime(this.filename);

                this.Sleep(.300);
                var x = db.First(y => y.Name == "Johnny Doe");
                x.FirstName = "John";
                Assert.NotEqual(edit_time, File.GetLastWriteTime(this.filename)); // should update on edit
            }
        }

        /// <summary>
        /// Test udno and redo - specifically when dealing with a string
        /// </summary>
        [Fact]
        public void DBUndoBasicChange_Name()
        {
            Console.WriteLine("Test: DBUndoBasicChange_Name");
            string path = Directory.GetCurrentDirectory();
            Console.WriteLine(path);
            var entry = new ExampleStoredItem("John", "Doe");

            using (var db = new MiniDB.DataBase<ExampleStoredItem>(this.filename, 1, 1))
            {
                Assert.False(File.Exists(this.filename));
                Assert.False(db.CanUndo, "Should not be able to undo from new db"); // should not be able to undo 0 changes
                db.Add(entry);
                var edit_time = File.GetLastWriteTime(this.filename);
                entry.FirstName = "John";

                Assert.Equal(edit_time, File.GetLastWriteTime(this.filename)); // should not update on no change
                Assert.True(db.CanUndo); // should be able to undo adding

                this.Sleep(.300);
                entry.FirstName = "Johnny";
                Assert.NotEqual(edit_time, File.GetLastWriteTime(this.filename));
                edit_time = File.GetLastWriteTime(this.filename);
                Assert.True(db.CanUndo); // should be able to undo rename

                this.Sleep(.300);
                db.Undo();
                Assert.Equal("John", entry.FirstName); // should be able to revert
                Assert.True(db.CanUndo); // should still be able to undo add
                Assert.NotEqual(edit_time, File.GetLastWriteTime(this.filename));
                edit_time = File.GetLastWriteTime(this.filename);

                this.Sleep(.300);
                db.Undo(); // re-remove camper
                Assert.Empty(db); // should now be empty
                Assert.False(db.CanUndo, "Should not be able to undo empty db"); // should not be able to undo when empty
                Assert.NotEqual(edit_time, File.GetLastWriteTime(this.filename));
                Assert.Throws<MiniDB.DBCannotUndoException>(() => db.Undo());
                edit_time = File.GetLastWriteTime(this.filename);

                this.Sleep(.300);
                db.Add(entry);
                var entry2 = new ExampleStoredItem("Jane", "Doe");
                db.Add(entry2);
                Assert.True(db.CanUndo);
                Assert.NotEqual(edit_time, File.GetLastWriteTime(this.filename));

                this.Sleep(.300);
                db.Undo();
                Assert.Single(db);
                Assert.True(db.CanUndo);
                Assert.NotEqual(edit_time, File.GetLastWriteTime(this.filename));
                Assert.Contains(db, x => x.ID == entry.ID);

                this.Sleep(.300);
                db.Add(entry2); // add camper
                Assert.True(db.CanUndo);
                Assert.Equal(2, db.Count);
                edit_time = File.GetLastWriteTime(this.filename);
                this.Sleep(.300);
                db.Remove(entry2); // remove camper
                Assert.NotEqual(edit_time, File.GetLastWriteTime(this.filename));
                Assert.Contains(db, x => x.ID == entry.ID);
                Assert.DoesNotContain(db, x => x.ID == entry2.ID);
                edit_time = File.GetLastWriteTime(this.filename);
                this.Sleep(.300);
                db.Undo(); // undo remove
                Assert.Contains(db, x => x.ID == entry.ID);
                Assert.Equal(2, db.Count);
            }
        }

        /// <summary>
        /// Test that undo and redo work correctly on an int
        /// </summary>
        [Fact]
        public void DBUndoBasicChange_Age()
        {
            Console.WriteLine("Test: DBUndoBasicChange_Age");
            string path = Directory.GetCurrentDirectory();
            Console.WriteLine(path);
            var entry = new ExampleStoredItem("John", "Doe");
            entry.Age = 3;

            using (var db = new MiniDB.DataBase<ExampleStoredItem>(this.filename, 1, 1))
            {
                Assert.False(File.Exists(this.filename));
                Assert.False(db.CanUndo, "Should not be able to undo from new db"); // should not be able to undo 0 changes
                db.Add(entry);
                var edit_time = File.GetLastWriteTime(this.filename);
                entry.Age = 3; // NOOP

                Assert.Equal(edit_time, File.GetLastWriteTime(this.filename)); // should not update on no change
                Assert.True(db.CanUndo); // should be able to undo adding

                this.Sleep(.300);
                entry.Age = 5;
                Assert.NotEqual(edit_time, File.GetLastWriteTime(this.filename));
                edit_time = File.GetLastWriteTime(this.filename);
                Assert.True(db.CanUndo); // should be able to undo rename

                this.Sleep(.300);
                db.Undo();
                Assert.Equal(3, entry.Age); // should be able to revert
                Assert.True(db.CanUndo); // should still be able to undo add
                Assert.True(db.CanRedo); // should still be able to undo add
                Assert.NotEqual(edit_time, File.GetLastWriteTime(this.filename));
                edit_time = File.GetLastWriteTime(this.filename);

                db.Redo();
                Assert.Equal(5, entry.Age);
                Assert.True(db.CanUndo); // should still be able to undo add
                Assert.False(db.CanRedo);
            }
        }

        /// <summary>
        /// Try to undo and redo basic changes and verify the DB modifies the data and the file correctly.
        /// </summary>
        [Fact]
        public void DBRedoUndoBasicChange()
        {
            Console.WriteLine("Test: DBRedoUndoBasicChange");
            var entry = new ExampleStoredItem("John", "Doe");

            using (var db = new MiniDB.DataBase<ExampleStoredItem>(this.filename, 1, 1))
            {
                Assert.False(File.Exists(this.filename), "File needs to be removed first");
                Assert.False(db.CanUndo, "Should not be able to undo empty db"); // should not be able to undo 0 changes
                Assert.False(db.CanRedo, "should not be able to redo empty db"); // should not be able to undo 0 changes
                db.Add(entry);
                var edit_time = File.GetLastWriteTime(this.filename);
                entry.FirstName = "John";

                Assert.Equal(edit_time, File.GetLastWriteTime(this.filename)); // should not update on no change
                Assert.True(db.CanUndo); // should be able to undo adding
                Assert.False(db.CanRedo, "should not be able to redo unless having run undo first");

                this.Sleep(.300);
                entry.FirstName = "Johnny";
                Assert.NotEqual(edit_time, File.GetLastWriteTime(this.filename));
                edit_time = File.GetLastWriteTime(this.filename);
                Assert.True(db.CanUndo); // should be able to undo rename

                this.Sleep(.300);
                db.Undo(); // udo name change
                Assert.NotEqual("Johnny", entry.FirstName);
                Assert.True(db.CanUndo); // should still be able to undo add
                Assert.True(db.CanRedo); // should now be able to re-do undo of name
                Assert.NotEqual(edit_time, File.GetLastWriteTime(this.filename));
                edit_time = File.GetLastWriteTime(this.filename);

                this.Sleep(.300);
                db.Redo(); // redo name change
                Assert.Equal("Johnny", entry.FirstName);
                Assert.True(db.CanUndo); // should still be able to undo add
                Assert.NotEqual(edit_time, File.GetLastWriteTime(this.filename));
                Assert.Throws<MiniDB.DBCannotRedoException>(() => db.Redo());
                edit_time = File.GetLastWriteTime(this.filename);

                var entry2 = new ExampleStoredItem("Jane", "Doe");
                entry2.Age = 3;
                db.Add(entry2);
                Assert.Equal(2, db.Count);
                db.Clear();
                Assert.Empty(db);
                Assert.False(db.CanRedo, "should not be able to redo on empty db");
                Assert.False(db.CanUndo, "should not be able to undo on empty db");
                edit_time = File.GetLastWriteTime(this.filename);

                db.Add(entry);
                db.Add(entry2);
                Assert.False(db.CanRedo, "should not be able to redo after adding");
                Assert.Equal(2, db.Count);
                db.Undo(); // undo add
                Assert.Single(db);
                Assert.True(db.CanRedo);
                edit_time = File.GetLastWriteTime(this.filename);
                this.Sleep(.300);
                db.Redo(); // redo add
                Assert.False(db.CanRedo, "should not be able to redo if all undo's have been undone");
                Assert.Equal(2, db.Count);
                Assert.NotEqual(edit_time, File.GetLastWriteTime(this.filename));

                db.Remove(entry);
                Assert.Single(db);
                Assert.False(db.CanRedo);
                Assert.True(db.CanUndo);
                db.Undo(); // undo remove
                Assert.Equal(2, db.Count);
                Assert.True(db.CanRedo);
                Assert.True(db.CanUndo);
                edit_time = File.GetLastWriteTime(this.filename);
                this.Sleep(.300);
                db.Redo();
                Assert.NotEqual(edit_time, File.GetLastWriteTime(this.filename));
                Assert.Single(db); // redo remove    

                Assert.True(db.CanUndo);
                entry2.Age = 4;
                Assert.True(db.CanUndo);
                db.Undo();
                Assert.Equal(3, entry2.Age);
            }
        }

        /// <summary>
        /// Test undo and redo in sequence
        /// </summary>
        [Fact]
        public void DBRedoUndoSmallStack()
        {
            Console.WriteLine("Test: DBRedoUndoSmallStack");
            var entry = new ExampleStoredItem(string.Empty, string.Empty);

            using (var db = new MiniDB.DataBase<ExampleStoredItem>(this.filename, 1, 1))
            {
                Assert.False(File.Exists(this.filename), "File needs to be removed first");
                Assert.False(db.CanUndo, "Should not be able to undo empty db"); // should not be able to undo 0 changes
                Assert.False(db.CanRedo, "should not be able to redo empty db"); // should not be able to undo 0 changes
                db.Add(entry);
                entry.FirstName += "J";

                Assert.True(db.CanUndo); // should be able to undo adding
                Assert.False(db.CanRedo, "should not be able to redo unless having run undo first");

                entry.FirstName += "o";
                Assert.True(db.CanUndo); // should be able to undo rename
                Assert.False(db.CanRedo);

                entry.FirstName += "h";
                Assert.True(db.CanUndo); // should be able to undo rename
                Assert.False(db.CanRedo);

                entry.FirstName += "n";
                Assert.True(db.CanUndo); // should be able to undo rename
                Assert.False(db.CanRedo);
                Assert.Equal("John", entry.FirstName);

                this.Sleep(.300);
                db.Undo(); // undo name change
                Assert.Equal("Joh", entry.FirstName);
                Assert.True(db.CanUndo); // should still be able to undo add
                Assert.True(db.CanRedo); // should now be able to re-do undo of name

                db.Redo(); // redo name change
                Assert.Equal("John", entry.FirstName);
                Assert.True(db.CanUndo); // should still be able to undo add
                Assert.False(db.CanRedo);

                db.Undo(); // udo name change
                Assert.Equal("Joh", entry.FirstName);
                Assert.True(db.CanUndo); // should still be able to undo add
                Assert.True(db.CanRedo); // should now be able to re-do undo of name

                db.Undo(); // udo name change
                Assert.Equal("Jo", entry.FirstName);
                Assert.True(db.CanUndo); // should still be able to undo add
                Assert.True(db.CanRedo); // should now be able to re-do undo of name

                db.Undo(); // udo name change
                Assert.Equal("J", entry.FirstName);
                Assert.True(db.CanUndo); // should still be able to undo add
                Assert.True(db.CanRedo); // should now be able to re-do undo of name

                db.Undo(); // udo name change
                Assert.Equal(string.Empty, entry.FirstName);
                Assert.True(db.CanUndo); // should still be able to undo add
                Assert.True(db.CanRedo); // should now be able to re-do undo of name

                db.Undo();
                Assert.Empty(db);
                Assert.False(db.CanUndo); // should still be able to undo add
                Assert.True(db.CanRedo); // should now be able to re-do undo of name

                db.Redo();
                Assert.Single(db);
                Assert.True(db.CanUndo); // should still be able to undo add
                Assert.True(db.CanRedo); // should now be able to re-do undo of name
                entry = db.First();

                db.Redo();
                Assert.Equal("J", entry.FirstName);
                Assert.True(db.CanUndo); // should still be able to undo add
                Assert.True(db.CanRedo); // should now be able to re-do undo of name

                db.Redo();
                Assert.Equal("Jo", entry.FirstName);
                Assert.True(db.CanUndo); // should still be able to undo add
                Assert.True(db.CanRedo); // should now be able to re-do undo of name

                db.Redo();
                Assert.Equal("Joh", entry.FirstName);
                Assert.True(db.CanUndo); // should still be able to undo add
                Assert.True(db.CanRedo); // should now be able to re-do undo of name

                db.Redo();
                Assert.Equal("John", entry.FirstName);
                Assert.True(db.CanUndo); // should still be able to undo add
                Assert.False(db.CanRedo); // should now be able to re-do undo of name
            }
        }

        /// <summary>
        /// Test that nested changes such as address.firstline or dict["key"].field can be undone as well
        /// </summary>
        [Fact]
        public void DBRedoUndoNestedChange()
        {
            Console.WriteLine("Test: DBRedoUndoNestedChange");
            var entry = new ExampleComplicatedStoredItem("John", "Doe");

            using (var db = new MiniDB.DataBase<ExampleComplicatedStoredItem>(this.filename, 1, 1))
            {
                Assert.False(File.Exists(this.filename), "File needs to be removed first");
                Assert.False(db.CanUndo, "Should not be able to undo empty db"); // should not be able to undo 0 changes
                Assert.False(db.CanRedo, "should not be able to redo empty db"); // should not be able to undo 0 changes
                db.Add(entry);
                var edit_time = File.GetLastWriteTime(this.filename);

                this.Sleep(.300);
                entry.Address.FirstLine = "PO Box";
                Assert.NotEqual(edit_time, File.GetLastWriteTime(this.filename));
                Assert.True(db.CanUndo);
                db.Undo();
                Assert.Equal(string.Empty, entry.Address.FirstLine);
            }
        }

        #endregion

        #region private methods
        /// <summary>
        /// Helper method to clean up db and transaction files if they exist
        /// </summary>
        private void Cleanup()
        {
            if (File.Exists(this.filename))
            {
                File.Delete(this.filename);
            }

            if (File.Exists(this.transactionsFile))
            {
                File.Delete(this.transactionsFile);
            }
        }

        /// <summary>
        /// After all the tests have finished, call the dispose method to cleanup
        /// </summary>
        private void Finally()
        {
            this.Dispose();
        }

        /// <summary>
        /// Run an asynchronous Task.Delay for the specified number of seconds and wait for it to finish
        /// </summary>
        /// <param name="seconds">The decimal number of seconds to wait (delay only supports millisecond precision)</param>
        private void Sleep(double seconds)
        {
            var t = Task.Run(async delegate
            {
                await Task.Delay((int)(seconds * 1000));
            });
            t.Wait();
        }
        #endregion
    }
}
