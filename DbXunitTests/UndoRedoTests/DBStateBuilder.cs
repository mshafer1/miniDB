using System.Linq;

namespace DbXunitTests.UndoRedoTests
{
    // inspired by discussion here: https://raygun.com/blog/unit-testing-patterns/#state
    internal class DBStateBuilder
    {
        private readonly MiniDB.DataBase _db;    
        
        public DBStateBuilder(MiniDB.DataBase db)
        {
            this._db = db;
        }

        public DBStateBuilder AddItem(MiniDB.IDBObject dbObjcet)
        {
            this._db.Add(dbObjcet);
            return this;
        }

        public DBStateBuilder Undo()
        {
            this._db.Undo();
            return this;
        }

        public DBStateBuilder Redo()
        {
            this._db.Redo();
            return this;
        }

        public DBStateBuilder EditItem(System.Action<MiniDB.IDBObject> edit)
        {
            var first = this._db.Single();

            edit(first);

            return this;
        }

        public MiniDB.DataBase Get_DB()
        {
            return this._db;
        }
    }
}
