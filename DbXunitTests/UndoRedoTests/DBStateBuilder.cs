using System.Linq;

using MiniDB.Interfaces;

namespace DbXunitTests.UndoRedoTests
{
    // inspired by discussion here: https://raygun.com/blog/unit-testing-patterns/#state
    internal class DBStateBuilder
    {
        private readonly MiniDB.DataBase db;

        public DBStateBuilder(MiniDB.DataBase db)
        {
            this.db = db;
        }

        public DBStateBuilder AddItem(IDBObject dbObjcet)
        {
            this.db.Add(dbObjcet);
            return this;
        }

        public DBStateBuilder Undo()
        {
            this.db.Undo();
            return this;
        }

        public DBStateBuilder Redo()
        {
            this.db.Redo();
            return this;
        }

        public DBStateBuilder EditItem(System.Action<IDBObject> edit)
        {
            var first = this.db.Single();

            edit(first);

            return this;
        }

        public MiniDB.DataBase Get_DB()
        {
            return this.db;
        }
    }
}
