using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbXunitTests
{
    /// <summary>
    /// 
    /// </summary>
    internal class ExampleComplicatedStoredItem : MiniDB.DatabaseObject
    {
        /// <summary>
        /// Create the object with a nested tracked object (Address)
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        public ExampleComplicatedStoredItem(string firstName, string lastName)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Address = new AddressClass();
            Address.PropertyChangedExtended += Address_PropertyChangedExtended;
        }

        private void Address_PropertyChangedExtended(object sender, MiniDB.PropertyChangedExtendedEventArgs e)
        {
            this.OnPropertyChangedExtended(nameof(Address) + "." + e.PropertyName, e.OldValue, e.NewValue, e.UndoableChange);
        }

        /// <summary>
        /// Gets or sets the first name
        /// </summary>
        public string FirstName
        {
            get => this.Get();
            set => this.Set(value);
        }

        /// <summary>
        /// Gets or sets the last name of the example person stored int the db
        /// </summary>
        public string LastName
        {
            get => this.Get();
            set => this.Set(value);
        }

        /// <summary>
        /// Gets or sets the age of the example person stored in the db
        /// </summary>
        public int? Age
        {
            get => this.Get();
            set => this.Set(value);
        }

        /// <summary>
        /// Gets the full name of the stored person
        /// </summary>
        public string Name
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }

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
