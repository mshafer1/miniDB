using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbXunitTests
{
    class ExampleComplicatedStoredItem : MiniDB.DatabaseObject
    {
        public ExampleComplicatedStoredItem(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
            Address = new AddressClass();
            Address.PropertyChangedExtended += Address_PropertyChangedExtended;
        }

        private void Address_PropertyChangedExtended(object sender, MiniDB.PropertyChangedExtendedEventArgs e)
        {
            this.OnPropertyChangedExtended(nameof(Address) + "." + e.PropertyName, e.OldValue, e.NewValue, e.UndoableChange);
        }

        public string FirstName { set => this.Set(value); get => this.Get(); }
        public string LastName { set => this.Set(value); get => this.Get(); }
        public int? Age { set => this.Set(value); get => this.Get(); }
        public string Name { get { return $"{FirstName} {LastName}"; } }
        public AddressClass Address { get; }
    }

    class AddressClass : MiniDB.DatabaseObject
    {
        public AddressClass()
        {
            FirstLine = SecondLine = City = State = Zip = "";
        }

        public string FirstLine { set => this.Set(value); get => this.Get(); }
        public string SecondLine { set => this.Set(value); get => this.Get(); }
        public string City { set => this.Set(value); get => this.Get(); }
        public string State { set => this.Set(value); get => this.Get(); }
        public string Zip { set => this.Set(value); get => this.Get(); }
    }
}
