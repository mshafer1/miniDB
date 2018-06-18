using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace DbXunitTests
{
    public class UndoRedoTests : IDisposable
    {
        private readonly string Filename;
        private readonly string transactionsFile;

        public UndoRedoTests()
        {
            Filename = "TestDB.json";
            transactionsFile = "transactions_" + Filename + ".data";

            // make sure it is clean
            Cleanup();
        }

        public void Dispose()
        {
            // clean up when finished
            Cleanup();
        }

        private void Cleanup()
        {
            if (File.Exists(Filename))
            {
                File.Delete(Filename);
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

        private void sleep(float seconds)
        {
            var t = Task.Run(async delegate
            {
                await Task.Delay((int)(seconds * 1000));
            });
            t.Wait();
        }

        #region test_methods

        [Fact]
        public void DBModelUndoRedoAddTest()
        {
            var entry = new ExampleStoredItem("John", "Doe");

            using (var db = new MiniDB.DataBase<ExampleStoredItem>(Filename))
            {
                Assert.False(File.Exists(Filename));
                sleep(1);
                db.Add(entry);
                Assert.True(File.Exists(Filename));
                Assert.True(File.Exists(transactionsFile));
                Assert.Single(db);
                Assert.True(db.CanUndo);
                Assert.False(db.CanRedo);
                var edit_time = File.GetLastWriteTime(Filename);

                sleep(1);
                db.Undo();
                Assert.True(File.Exists(Filename));
                Assert.True(File.Exists(transactionsFile));
                Assert.Empty(db);
                Assert.NotEqual(edit_time, File.GetLastWriteTime(Filename)); // should update on edit
                Assert.False(db.CanUndo);
                Assert.True(db.CanRedo);
                edit_time = File.GetLastWriteTime(Filename);

                sleep(1);
                db.Redo();
                Assert.True(db.CanUndo);
                Assert.False(db.CanRedo);
                Assert.True(File.Exists(Filename));
                Assert.True(File.Exists(transactionsFile));
                Assert.Single(db);
                Assert.NotEqual(edit_time, File.GetLastWriteTime(Filename)); // should update on edit
                Assert.Equal("John", db.First().FirstName);
                Assert.Equal("Doe", db.First().LastName);
            }
        }

        [Fact]
        public void DBModelRegistersAddedItemsForPropertyChangedEventsTest()
        {
            var entry = new ExampleStoredItem("John", "Doe");
            using (var db = new MiniDB.DataBase<ExampleStoredItem>(Filename))
            {
                Assert.False(File.Exists(Filename));

                sleep(1);
                db.Add(entry);
                Assert.True(File.Exists(Filename));
                Assert.True(File.Exists(transactionsFile));
                var edit_time = File.GetLastWriteTime(Filename);

                sleep(1);
                entry.FirstName = "John";
                Assert.Equal(edit_time, File.GetLastWriteTime(Filename)); // should not update on no change

                sleep(1);
                entry.FirstName = "Johnny";
                Assert.NotEqual(edit_time, File.GetLastWriteTime(Filename)); // should update on edit
                edit_time = File.GetLastWriteTime(Filename);

                sleep(1);
                var x = db.First(y => y.Name == "Johnny Doe");
                x.FirstName = "John";
                Assert.NotEqual(edit_time, File.GetLastWriteTime(Filename)); // should update on edit
            }
        }

        [Fact]
        public void DBUndoBasicChange_Name()
        {
            string path = Directory.GetCurrentDirectory();
            Console.WriteLine(path);
            var entry = new ExampleStoredItem("John", "Doe");

            using (var db = new MiniDB.DataBase<ExampleStoredItem>(Filename))
            {
                Assert.False(File.Exists(Filename));
                Assert.False(db.CanUndo, "Should not be able to undo from new db"); // should not be able to undo 0 changes
                db.Add(entry);
                var edit_time = File.GetLastWriteTime(Filename);
                entry.FirstName = "John";

                Assert.Equal(edit_time, File.GetLastWriteTime(Filename)); // should not update on no change
                Assert.True(db.CanUndo); // should be able to undo adding

                sleep(1);
                entry.FirstName = "Johnny";
                Assert.NotEqual(edit_time, File.GetLastWriteTime(Filename));
                edit_time = File.GetLastWriteTime(Filename);
                Assert.True(db.CanUndo); // should be able to undo rename

                sleep(1);
                db.Undo();
                Assert.Equal("John", entry.FirstName); // should be able to revert
                Assert.True(db.CanUndo); // should still be able to undo add
                Assert.NotEqual(edit_time, File.GetLastWriteTime(Filename));
                edit_time = File.GetLastWriteTime(Filename);

                sleep(1);
                db.Undo(); // re-remove camper
                Assert.Empty(db); // should now be empty
                Assert.False(db.CanUndo, "Should not be able to undo empty db"); // should not be able to undo when empty
                Assert.NotEqual(edit_time, File.GetLastWriteTime(Filename));
                Assert.Throws<MiniDB.DBCannotUndoException>(() => db.Undo());
                edit_time = File.GetLastWriteTime(Filename);

                sleep(1);
                db.Add(entry);
                var entry2 = new ExampleStoredItem("Jane", "Doe");
                db.Add(entry2);
                Assert.True(db.CanUndo);
                Assert.NotEqual(edit_time, File.GetLastWriteTime(Filename));

                sleep(1);
                db.Undo();
                Assert.Single(db);
                Assert.True(db.CanUndo);
                Assert.NotEqual(edit_time, File.GetLastWriteTime(Filename));
                Assert.Contains(db, x => x.ID == entry.ID);

                sleep(1);
                db.Add(entry2); // add camper
                Assert.True(db.CanUndo);
                Assert.Equal(2, db.Count);
                edit_time = File.GetLastWriteTime(Filename);
                sleep(1);
                db.Remove(entry2); // remove camper
                Assert.NotEqual(edit_time, File.GetLastWriteTime(Filename));
                Assert.Contains(db, x => x.ID == entry.ID);
                Assert.DoesNotContain(db, x => x.ID == entry2.ID);
                edit_time = File.GetLastWriteTime(Filename);
                sleep(1);
                db.Undo(); // undo remove
                Assert.Contains(db, x => x.ID == entry.ID);
                Assert.Equal(2, db.Count);
            }
        }

        [Fact]
        public void DBUndoBasicChange_Age()
        {
            string path = Directory.GetCurrentDirectory();
            Console.WriteLine(path);
            var entry = new ExampleStoredItem("John", "Doe");
            entry.Age = 3;

            using (var db = new MiniDB.DataBase<ExampleStoredItem>(Filename))
            {
                Assert.False(File.Exists(Filename));
                Assert.False(db.CanUndo, "Should not be able to undo from new db"); // should not be able to undo 0 changes
                db.Add(entry);
                var edit_time = File.GetLastWriteTime(Filename);
                entry.Age = 3; // NOOP

                Assert.Equal(edit_time, File.GetLastWriteTime(Filename)); // should not update on no change
                Assert.True(db.CanUndo); // should be able to undo adding

                sleep(1);
                entry.Age = 5;
                Assert.NotEqual(edit_time, File.GetLastWriteTime(Filename));
                edit_time = File.GetLastWriteTime(Filename);
                Assert.True(db.CanUndo); // should be able to undo rename

                sleep(1);
                db.Undo();
                Assert.Equal(3, entry.Age); // should be able to revert
                Assert.True(db.CanUndo); // should still be able to undo add
                Assert.True(db.CanRedo); // should still be able to undo add
                Assert.NotEqual(edit_time, File.GetLastWriteTime(Filename));
                edit_time = File.GetLastWriteTime(Filename);

                db.Redo();
                Assert.Equal(5, entry.Age);
                Assert.True(db.CanUndo); // should still be able to undo add
                Assert.False(db.CanRedo);
            }
        }

        [Fact]
        public void DBRedoUndoBasicChange()
        {
            var entry = new ExampleStoredItem("John", "Doe");

            using (var db = new MiniDB.DataBase<ExampleStoredItem>(Filename))
            {
                Assert.False(File.Exists(Filename), "File needs to be removed first");
                Assert.False(db.CanUndo, "Should not be able to undo empty db"); // should not be able to undo 0 changes
                Assert.False(db.CanRedo, "should not be able to redo empty db"); // should not be able to undo 0 changes
                db.Add(entry);
                var edit_time = File.GetLastWriteTime(Filename);
                entry.FirstName = "John";

                Assert.Equal(edit_time, File.GetLastWriteTime(Filename)); // should not update on no change
                Assert.True(db.CanUndo); // should be able to undo adding
                Assert.False(db.CanRedo, "should not be able to redo unless having run undo first");

                sleep(1);
                entry.FirstName = "Johnny";
                Assert.NotEqual(edit_time, File.GetLastWriteTime(Filename));
                edit_time = File.GetLastWriteTime(Filename);
                Assert.True(db.CanUndo); // should be able to undo rename

                sleep(1);
                db.Undo(); // udo name change
                Assert.NotEqual("Johnny", entry.FirstName);
                Assert.True(db.CanUndo); // should still be able to undo add
                Assert.True(db.CanRedo); // should now be able to re-do undo of name
                Assert.NotEqual(edit_time, File.GetLastWriteTime(Filename));
                edit_time = File.GetLastWriteTime(Filename);

                sleep(1);
                db.Redo(); // redo name change
                Assert.Equal("Johnny", entry.FirstName);
                Assert.True(db.CanUndo); // should still be able to undo add
                Assert.NotEqual(edit_time, File.GetLastWriteTime(Filename));
                Assert.Throws<MiniDB.DBCannotRedoException>(() => db.Redo());
                edit_time = File.GetLastWriteTime(Filename);

                var entry2 = new ExampleStoredItem("Jane", "Doe");
                entry2.Age = 3;
                db.Add(entry2);
                Assert.Equal(2, db.Count);
                db.Clear();
                Assert.Empty(db);
                Assert.False(db.CanRedo, "should not be able to redo on empty db");
                Assert.False(db.CanUndo, "should not be able to undo on empty db");
                edit_time = File.GetLastWriteTime(Filename);

                db.Add(entry);
                db.Add(entry2);
                Assert.False(db.CanRedo, "should not be able to redo after adding");
                Assert.Equal(2, db.Count);
                db.Undo(); // undo add
                Assert.Single(db);
                Assert.True(db.CanRedo);
                edit_time = File.GetLastWriteTime(Filename);
                sleep(1);
                db.Redo(); // redo add
                Assert.False(db.CanRedo, "should not be able to redo if all undo's have been undone");
                Assert.Equal(2, db.Count);
                Assert.NotEqual(edit_time, File.GetLastWriteTime(Filename));

                db.Remove(entry);
                Assert.Single(db);
                Assert.False(db.CanRedo);
                Assert.True(db.CanUndo);
                db.Undo(); // undo remove
                Assert.Equal(2, db.Count);
                Assert.True(db.CanRedo);
                Assert.True(db.CanUndo);
                edit_time = File.GetLastWriteTime(Filename);
                sleep(1);
                db.Redo();
                Assert.NotEqual(edit_time, File.GetLastWriteTime(Filename));
                Assert.Single(db); // redo remove    

                Assert.True(db.CanUndo);
                entry2.Age = 4;
                Assert.True(db.CanUndo);
                db.Undo();
                Assert.Equal(3, entry2.Age);
            }
        }

        [Fact]
        public void DBRedoUndoSmallStack()
        {
            var entry = new ExampleStoredItem("", "");

            using (var db = new MiniDB.DataBase<ExampleStoredItem>(Filename))
            {
                Assert.False(File.Exists(Filename), "File needs to be removed first");
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


                sleep(1);
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
                Assert.Equal("", entry.FirstName);
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

        [Fact]
        public void DBRedoUndoNestedChange()
        {
            var entry = new ExampleComplicatedStoredItem("John", "Doe");

            using (var db = new MiniDB.DataBase<ExampleComplicatedStoredItem>(Filename))
            {
                Assert.False(File.Exists(Filename), "File needs to be removed first");
                Assert.False(db.CanUndo, "Should not be able to undo empty db"); // should not be able to undo 0 changes
                Assert.False(db.CanRedo, "should not be able to redo empty db"); // should not be able to undo 0 changes
                db.Add(entry);
                var edit_time = File.GetLastWriteTime(Filename);

                sleep(1);
                entry.Address.FirstLine = "PO Box";
                Assert.NotEqual(edit_time, File.GetLastWriteTime(Filename));
                Assert.True(db.CanUndo);
                db.Undo();
                Assert.Equal("", entry.Address.FirstLine);
            }
        }

        #endregion
    }


}
