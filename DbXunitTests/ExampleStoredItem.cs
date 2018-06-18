using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using MiniDB;

namespace DbXunitTests
{
    class ExampleStoredItem : MiniDB.DatabaseObject
    {
        public ExampleStoredItem(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public string   FirstName   { set => this.Set(value); get => this.Get(); }
        public string   LastName    { set => this.Set(value); get => this.Get(); }
        public int?     Age         { set => this.Set(value); get => this.Get(); }
        public string   Name        { get { return $"{FirstName} {LastName}"; } }
    }
}
