using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbXunitTests.UndoRedoTests
{
    // inspired by discussion here: https://raygun.com/blog/unit-testing-patterns/#state
    class DBStateBuilder
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

        public MiniDB.DataBase Get_DB()
        {
            return this._db;
        }
    }
}
