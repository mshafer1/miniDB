using MiniDB;

namespace TestProject
{
    class Program
    {
        class DbObject : BaseDBObject, MiniDB.Interfaces.IDBObject
        {
            public string Name { get => this.Get(); set => this.Set(value); }
        }

        static void Main(string[] args)
        {
            var test = new MiniDB.DataBase("test.json", 1.0f, 1.0f);

            var blah = new DbObject();
            blah.Name = "test";
            test.Add(blah);
            blah.Name = "John Doe";
            test.Undo();
            test.Undo();
        }
    }
}
