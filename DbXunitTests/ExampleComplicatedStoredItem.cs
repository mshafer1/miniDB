using System.Diagnostics.CodeAnalysis;

namespace DbXunitTests
{
    /// <summary>
    /// Example of a storable item with a nested property (Address)
    /// </summary>
    public class ExampleComplicatedStoredItem : MiniDB.BaseDBObject
    {
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExampleComplicatedStoredItem" /> class.
        /// Create the object with a nested tracked object (Address)
        /// Address defaults to blank
        /// </summary>
        /// <param name="firstName">The first name of the stored person</param>
        /// <param name="lastName">The last name of the stored person</param>
        public ExampleComplicatedStoredItem(string firstName, string lastName)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Address = new AddressClass();
        }
        #endregion

        #region properties

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
                return $"{this.FirstName} {this.LastName}";
            }
        }

        /// <summary>
        /// Gets Nested property - this address is also tracked at a sub item level
        /// </summary>
        public AddressClass Address { get; }
        #endregion
    }

    /// <summary>
    /// Example of a supported nested property
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1402:FileMayOnlyContainASingleType", Justification = "Quick example - should go in own file in real world")]
    public class AddressClass : MiniDB.BaseDBObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddressClass"/> class.
        /// </summary>
        public AddressClass()
        {
            this.FirstLine = this.SecondLine = this.City = this.State = string.Empty;
            this.Zip = new Zip();
        }

        /// <summary>
        /// Gets or sets the the address first line
        /// </summary>
        public string FirstLine
        {
            get => this.Get();
            set => this.Set(value);
        }

        /// <summary>
        /// Gets or sets the second line of the address
        /// </summary>
        public string SecondLine
        {
            get => this.Get();
            set => this.Set(value);
        }

        /// <summary>
        /// Gets or sets the city
        /// </summary>
        public string City
        {
            get => this.Get();
            set => this.Set(value);
        }

        /// <summary>
        /// Gets or sets the state
        /// </summary>
        public string State
        {
            get => this.Get();
            set => this.Set(value);
        }

        /// <summary>
        /// Gets or sets the zip code
        /// </summary>
        public Zip Zip
        {
            get => this.Get();
            set => this.Set(value);
        }
    }

    // create a second nested item
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1402:FileMayOnlyContainASingleType", Justification = "Quick example - should go in own file in real world")]
    public class Zip : MiniDB.BaseDBObject
    {
        public Zip()
        {
            this.Value = 0;
        }

        public int Value
        {
            get => this.Get();
            set => this.Set(value);
        }
    }
}