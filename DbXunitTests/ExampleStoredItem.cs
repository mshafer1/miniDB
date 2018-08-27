using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbXunitTests
{
    /// <summary>
    /// An example of an object class that can be stored in a MiniDB. This is used in the unit tests.
    /// </summary>
    internal class ExampleStoredItem : MiniDB.DatabaseObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExampleStoredItem" /> class.
        /// </summary>
        /// <param name="firstName">Person's first name</param>
        /// <param name="lastName">Person's last name</param>
        /// <param name="id">ID in DB</param>
        /// <param name="age">how old is the person</param>
        public ExampleStoredItem(string firstName, string lastName, MiniDB.ID id, int? age) : base(id)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Age = age;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExampleStoredItem" /> class.
        /// Age defaults to 0
        /// </summary>
        public ExampleStoredItem() : this(string.Empty, string.Empty, null, 0)
        {
            this.FirstName = this.LastName = string.Empty;
            this.Age = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExampleStoredItem" /> class.
        /// Age defaults to 0
        /// </summary>
        /// <param name="id">The ID value to use</param>
        public ExampleStoredItem(MiniDB.ID id) : this(string.Empty, string.Empty, id, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExampleStoredItem" /> class.
        /// Age defaults to 0
        /// </summary>
        /// <param name="firstName">First name to store</param>
        /// <param name="lastName">Last name to store</param>
        public ExampleStoredItem(string firstName, string lastName) : this(firstName, lastName, null, 0)
        {
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
        /// NOTE: All properties must be nullabe because this.Get and Set use null for the default if not provided yet.
        /// </summary>
        public int? Age
        {
            get => this.Get();
            set => this.Set(value);
        }

        /// <summary>
        /// Gets the full name of the stored person
        /// </summary>
        public string Name => $"{FirstName} {LastName}";
    }
}
