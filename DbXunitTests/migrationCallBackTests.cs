using System;
using System.Linq;

using Newtonsoft.Json.Linq;
using Xunit;

namespace DbXunitTests
{
    /// <summary>
    /// Test the basic functionality of the migration call backs
    /// </summary>
    public class MigrationCallBackTests : IDisposable
    {
        /// <summary>
        /// the expected base id value.
        /// </summary>
        private const int ExpectedID = 1955653895;

        /// <summary>
        /// the expected hardware component
        /// </summary>
        private const ulong ExpectedHardware = 3621229645820015414;

        /// <summary>
        /// a simple db stored in json to be able to convert
        /// </summary>
        private readonly string exampleDBJson = "{ '$id': '1', 'DBVersion': 1.5, 'Collection': [{  '$id': '2',  'name': 'John Doe',  'ID': {   '$id': '3',   'id': " + $"{ExpectedID}" + ",   'hardwareComponent': " + $"{ExpectedHardware}" + "  } }]}";

        /// <summary>
        /// db filename to cleaup
        /// </summary>
        private readonly string filename = "testMigrationDB.json";

        /// <summary>
        /// transactions file to cleanup
        /// </summary>
        private readonly string transactionsFilename = "transaction_testMigrationDB.json";

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationCallBackTests" /> class.
        /// Cleanup any files left over due to an error, then create basic db from example json.
        /// </summary>
        public MigrationCallBackTests()
        {
            this.Cleanup();
            System.IO.File.WriteAllText(this.filename, this.exampleDBJson);
        }

        /// <summary>
        /// Cleanup in between tests
        /// </summary>
        public void Dispose()
        {
            this.Cleanup();
        }

        /// <summary>
        /// Test that too old of a db version throws a DBCreationException error
        /// </summary>
        [Fact]
        public void TestTooOldVersionThrows()
        {
            Assert.Throws<MiniDB.DBCreationException>(() =>
            {
                // minimum compatible version is greater than stored DBVersion value.
                using (var db = new MiniDB.DataBase<ExampleStoredItem>(this.filename, 1.6f, 1.6f))
                {
                    // NO-OP
                }
            });
        }

        /// <summary>
        /// Test that too new of a version throws a DBCreationException error
        /// </summary>
        [Fact]
        public void TestTooNewVersionThrows()
        {
            Assert.Throws<MiniDB.DBCreationException>(() =>
            {
                // stored DBVersion is greater than current version
                using (var db = new MiniDB.DataBase<ExampleStoredItem>(this.filename, 1.4f, 1.0f))
                {
                    // NO-OP
                }
            });
        }

        /// <summary>
        /// Test that when a migration call back is provided, it gets called and performs the expected migration.
        /// </summary>
        [Fact]
        public void TestMigrationCallBack()
        {
            using (var db = new MiniDB.DataBase<ExampleStoredItem>(this.filename, 2.0f, 1.0f, this.Migrate))
            {
                Assert.Single(db);
                Assert.Equal("John", db.First().FirstName);
                Assert.Equal("Doe", db.First().LastName);
                var id = new MiniDB.ID(ExpectedID, ExpectedHardware);
                Assert.Equal(id, db.First().ID);
            }
        }

        /// <summary>
        /// Helper method to perform the migration from version 1.5 to version 2.0
        /// </summary>
        /// <param name="input">The DBMigrationParameters that pass in the collection to be modified and the version info</param>
        /// <returns>The JToken object that was passed in after copying values to appropriate locations</returns>
        private JToken Migrate(MiniDB.DataBase<ExampleStoredItem>.DBMigrationParameters input)
        {
            if (input.TargetVersion != 2.0)
            {
                throw new Exception($"Cannot convert from {input.OldVersion} to {input.TargetVersion}");
            }
            if (input.OldVersion == 1.5)
            {
                // iterate over items in loaded DB and perform the update
                foreach (var child in input.Collection.Children())
                {
                    var id = new MiniDB.ID(child);
                    string firstName = child["name"].ToString().Split(' ').First();
                    string lastName = string.Concat(child["name"].ToString().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Skip(1));

                    child[nameof(ExampleStoredItem.FirstName)] = firstName;
                    child[nameof(ExampleStoredItem.LastName)] = lastName;
                }
                return input.Collection;
            }
            // add more previous versions if needed
            else
            {
                throw new Exception($"Cannot convert from {input.OldVersion} to {input.TargetVersion}");
            }
        }

        /// <summary>
        /// Delete the files so that this/these test(s) start out with a clean state
        /// </summary>
        private void Cleanup()
        {
            if (System.IO.File.Exists(this.filename))
            {
                System.IO.File.Delete(this.filename);
            }

            if (System.IO.File.Exists(this.transactionsFilename))
            {
                System.IO.File.Delete(this.transactionsFilename);
            }
        }
    }
}
