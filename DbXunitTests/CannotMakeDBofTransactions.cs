using Xunit;

namespace DbXunitTests
{
    /// <summary>
    /// Tests to make sure that it is impossible to create an instance of a DB <see cref="MiniDB.DataBase{T}"/> with Transactions <see cref="MiniDB.DBTransaction{T}"/> as the type.
    /// </summary>
    public class CannotMakeDBofTransactions
    {
        /// <summary>
        /// Assert that creating a DB of DBTransactions throws
        /// </summary>
        [Fact]
        public void TestCannotMakeDBofTransactions()
        {
            Assert.Throws<MiniDB.DBCreationException>(() => { var x = new MiniDB.DataBase<MiniDB.DBTransaction<ExampleStoredItem>>("testDB", 1, 1, null); }); // TODO: add storage strategy
        }
    }
}
