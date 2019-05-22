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
            this.Address.PropertyChangedExtended += this.Address_PropertyChangedExtended;
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
                return $"{FirstName} {LastName}";
            }
        }

        /// <summary>
        /// Gets Nested property - this address is also tracked at a sub item level
        /// </summary>
        public AddressClass Address { get; }
        #endregion

        #region private methods

        /// <summary>
        /// When an address field is changed - raise an event on this object to
        /// </summary>
        /// <param name="sender">the object that raised the event - expected to be this address</param>
        /// <param name="e">the event args</param>
        private void Address_PropertyChangedExtended(object sender, MiniDB.PropertyChangedExtendedEventArgs e)
        {
            var address = sender as AddressClass;
            if (address == null || !object.ReferenceEquals(address, this.Address))
            {
                // not the expected sender - not really an error situation in this case, so just return.
                return;
            }

            this.OnPropertyChangedExtended(nameof(this.Address) + "." + e.PropertyName, e.OldValue, e.NewValue, e.UndoableChange);
        }

        #endregion
    }

    /// <summary>
    /// Example of an supported nested property
    /// </summary>
    public class AddressClass : MiniDB.BaseDBObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddressClass"/> class.
        /// </summary>
        public AddressClass()
        {
            this.FirstLine = this.SecondLine = this.City = this.State = this.Zip = string.Empty;
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
        public string Zip
        {
            get => this.Get();
            set => this.Set(value);
        }
    }
}