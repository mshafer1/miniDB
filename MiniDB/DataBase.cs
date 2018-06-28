using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

using Newtonsoft.Json;

namespace MiniDB
{
    /// <summary>
    /// Database Template: template class for persisten observable collection with undo/redo
    /// </summary>
    /// <typeparam name="T">The class type to create an observable collection of (must by a child class of DatabaseObject)</typeparam>
    public class DataBase<T> : ObservableCollection<T>, IDisposable where T : DatabaseObject
    {
        #region fields
        /// <summary>
        /// lock object - this is used to lock the current database from attempting to edit the data at the same time.
        /// </summary>
        protected static readonly object Locker = new object();

        /// <summary>
        /// The minimum compatible version that can be migrated when reloaded from disk
        /// </summary>
        protected readonly float MinimumCompatibleVersion = 1;

        /// <summary>
        /// Json Serializer Settings object used to specify how NewtonSoft should cache objects
        /// </summary>
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        /// <summary>
        /// http://www.albahari.com/threading/part2.aspx#_Mutex
        /// Create mutex in constructor - name it so that only one instance of db class can be accessing file
        ///  - this allows for multiple instances of a DB, but only one of a given type accessing a given file
        /// </summary>
        private Mutex mut = null;
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="{T}" /> class.
        /// Create instance of database - if file exists, load collection from it; else, create new empty collection
        /// </summary>
        /// <param name="filename">The filename or path to store the collection in</param>
        /// <param name="databaseVersion">The current version of the database (stored only to one decimal place and max value of 25.5 - if unsure what to use, put 0.1 for now</param>
        /// <param name="minimumCompatibleVersion">The mimum compatible version - if unsure what to use, put 0.1 for now</param>
        public DataBase(string filename, float databaseVersion, float minimumCompatibleVersion) : base()
        {
            this.DBVersion = databaseVersion;
            this.MinimumCompatibleVersion = minimumCompatibleVersion;
            lock (Locker)
            {
                filename = Path.GetFullPath(filename); // use the full system path - especially for mutex to know if it needs to lock that file or not
                string mutex_file_path = filename;
                mutex_file_path = mutex_file_path.Replace("\\", "_");
                string mutex_name = string.Format(@"{0}<{1}>:{2}", nameof(DataBase<T>), typeof(T).Name, mutex_file_path); // from https://stackoverflow.com/a/2534867
                string global_lock_mutex_name = @"Global\" + mutex_name;

                // from https://stackoverflow.com/a/3111740
                // try to get existing mutex from syste
                try
                {
                    this.mut = System.Threading.Mutex.OpenExisting(global_lock_mutex_name);

                    // mutex already exists
                    throw new DBCreationException("Another application instance is using that DB!\n\tError from: " + mutex_name);
                }
                catch (WaitHandleCannotBeOpenedException)
                {
                    this.mut = new System.Threading.Mutex(false, global_lock_mutex_name);
                }

                // acquire the lock from the mutex - this is release in dispose
                if (!this.mut.WaitOne(TimeSpan.FromSeconds(5), false))
                {
                    throw new DBCreationException("Another application instance is using that DB!\n\tError from: " + mutex_name);
                }

                this.Filename = filename;

                // TODO: I doubt this works, . . .
                if (typeof(T) == typeof(DBTransaction<T>))
                {
                    // prevent recursion
                    return;
                }

                string transactionFilename = string.Format(@"{0}\transactions_{1}.data", Path.GetDirectoryName(this.Filename), Path.GetFileName(this.Filename));
                this.Transactions_DB = this.getTransactionsDB(transactionFilename);
                this.Transactions_DB.CollectionChanged += this.DataBase_TransactionsChanged;

                this.LoadFile(filename, true);

                this.CollectionChanged += this.DataBase_CollectionChanged;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="{T}"/> class.
        /// Default constructor (allow Newtonsoft to create object without parameters
        /// </summary>
        [JsonConstructor]
        internal DataBase() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="{T}"/> class.
        /// </summary>
        /// <param name="filename">the filename (or path) to cache this collection in</param>
        /// <param name="databaseVersion">the current version of the database</param>
        /// <param name="minimumCompatibleVersion">the minimum version of a database that can be upgraded to the current version</param>
        /// <param name="base_case">used to seperate this constructor (used to create the transactions DB) from the base call</param>
        protected DataBase(string filename, float databaseVersion, float minimumCompatibleVersion, bool base_case) : base()
        {
            this.DBVersion = databaseVersion;
            this.Filename = filename;
            this.LoadFile(filename, false);
        }
        #endregion

        #region destructors
        /// <summary>
        /// Finalizes an instance of the <see cref="DataBase"/> class.
        /// double check that this gets disposed of properly
        /// </summary>
        ~DataBase()
        {
            this.Dispose();
        }
        #endregion

        #region properties

        /// <summary>
        /// Gets a value indicating whether the collection currently can undo a recent transaction
        /// </summary>
        public bool CanUndo
        {
            get
            {
                return this.Transactions_DB.Count() > 0 && this.Transactions_DB.Count() >
                    (2 * this.Transactions_DB.Count(x => x.TransactionType == TransactionType.Undo));
            }
        }

        /// <summary>
        /// Gets a value indicating whether the collection currently can redo an undone change
        /// </summary>
        public bool CanRedo
        {
            get
            {
                // TODOne: this should be number of immediate redo's is less than number of next immediate undo's
                //  bool result = true;
                var redos_count = this.countRecentTransactions(TransactionType.Redo);
                Func<DBTransaction<T>, bool> matcher = x => x.TransactionType == TransactionType.Undo && x.Active == true;
                var undos_count = this.countRecentTransactions(matcher, this.Transactions_DB.Skip(redos_count * 2));
                return undos_count > 0;
            }
        }

        /// <summary>
        /// Gets the current DB Version - while this is a float, the current implementation of storing and reloading this only supports one decimal point. Max value of 25.5.
        /// </summary>
        [JsonProperty]
        public float DBVersion { get; internal set; }

        /// <summary>
        /// Gets the filename/path that is used to cache the collection in
        /// </summary>
        protected string Filename { get; private set; }

        /// <summary>
        /// Gets or sets the databse for caching transactions for undo/redo.
        /// </summary>
        private DataBase<DBTransaction<T>> Transactions_DB { get; set; }
        #endregion

        #region public methods        
        /// <summary>
        /// Clear the mutex when a using statement leaves scope (or destructor is called)
        /// </summary>
        public void Dispose()
        {
            if (this.mut != null)
            {
                this.mut.ReleaseMutex();
                this.mut.Close();
                this.mut = null;
            }
        }

        /// <summary>
        /// Undo last undoable transaction and raise property changed events on changed item.
        /// TODO: make sure to use locker everywhere that it is messing with the data
        /// </summary>
        public void Undo()
        {
            if (!this.CanUndo)
            {
                throw new DBCannotUndoException("Cannot undo at this time");
            }

            // get last transaction
            DBTransaction<T> last_transaction = this.GetLastTransaction(TransactionType.Undo, x => x.Active == true);

            T transactedItem = null;

            // get the mutex
            lock (Locker)
            {
                // deregister Changed handler
                this.CollectionChanged -= this.DataBase_CollectionChanged;
                bool registerItem = true;
                DBTransaction<T> undoTransaction;
                try
                {
                    if (last_transaction.TransactionType == TransactionType.Modify)
                    {
                        // get item from last transaction
                        transactedItem = this.FirstOrDefault(x => x.ID == last_transaction.Item_ID);
                        if (transactedItem == null)
                        {
                            throw new DBCannotUndoException(string.Format("Failed to find item with ID {1} to undo property {0}", last_transaction.changed_property, last_transaction.Item_ID));
                        }

                        transactedItem.PropertyChangedExtended -= this.DataBaseItem_PropertyChanged;

                        SetProperty(last_transaction, transactedItem);

                        // Create Undo transaction                        
                        undoTransaction = new DBTransaction<T>()
                        {
                            TransactionType = TransactionType.Undo,
                            Item_ID = transactedItem.ID,
                            Transacted_item = null,
                            property_old = last_transaction.property_new,
                            property_new = last_transaction.property_old,
                            changed_property = last_transaction.changed_property
                        };
                    }
                    else if (last_transaction.TransactionType == TransactionType.Add)
                    {
                        transactedItem = this.FirstOrDefault(x => x.ID == last_transaction.Item_ID);
                        if (transactedItem == null)
                        {
                            throw new DBCannotUndoException(string.Format("Failed to find item with ID {0} to remove", last_transaction.Item_ID));
                        }

                        transactedItem.PropertyChangedExtended -= this.DataBaseItem_PropertyChanged;
                        registerItem = false;
                        if (last_transaction.Transacted_item == null)
                        {
                            throw new DBCannotUndoException();
                        }

                        this.Remove(transactedItem);
                        undoTransaction = new DBTransaction<T>()
                        {
                            TransactionType = TransactionType.Undo,
                            Item_ID = transactedItem.ID,
                            Transacted_item = transactedItem,
                            property_old = null,
                            property_new = null,
                            changed_property = DBTransaction<T>.ITEM_REMOVED
                        };
                    }
                    else if (last_transaction.TransactionType == TransactionType.Delete)
                    {
                        if (last_transaction.Transacted_item == null)
                        {
                            throw new DBCannotUndoException();
                        }

                        transactedItem = last_transaction.Transacted_item;
                        if (transactedItem == null)
                        {
                            throw new DBCannotUndoException(string.Format("Failed to find item to re-add", last_transaction.changed_property, last_transaction.Item_ID));
                        }

                        transactedItem.PropertyChangedExtended -= this.DataBaseItem_PropertyChanged;

                        this.Add(transactedItem);

                        undoTransaction = new DBTransaction<T>()
                        {
                            TransactionType = TransactionType.Undo,
                            Item_ID = transactedItem.ID,
                            Transacted_item = last_transaction.Transacted_item,
                            property_old = null,
                            property_new = null,
                            changed_property = DBTransaction<T>.ITEM_ADDED
                        };
                    }
                    else if (last_transaction.TransactionType == TransactionType.Redo)
                    {
                        // TODOne: if it was a REDO - there is more involved
                        if (last_transaction.changed_property == DBTransaction<T>.ITEM_REMOVED)
                        {
                            transactedItem = last_transaction.Transacted_item;
                            this.Add(transactedItem);
                            undoTransaction = new DBTransaction<T>
                            {
                                Item_ID = transactedItem.ID,
                                changed_property = DBTransaction<T>.ITEM_ADDED,
                                TransactionType = TransactionType.Undo
                            };
                        }
                        else if (last_transaction.changed_property == DBTransaction<T>.ITEM_ADDED)
                        {
                            transactedItem = last_transaction.Transacted_item;
                            if (transactedItem == null)
                            {
                                throw new DBCannotUndoException(string.Format("Failed to find item with ID {1} to redo property {0}", last_transaction.changed_property, last_transaction.Item_ID));
                            }

                            transactedItem.PropertyChangedExtended -= this.DataBaseItem_PropertyChanged;
                            this.Remove(transactedItem);
                            registerItem = false;
                            undoTransaction = new DBTransaction<T>
                            {
                                changed_property = DBTransaction<T>.ITEM_REMOVED,
                                Item_ID = transactedItem.ID,
                                Transacted_item = transactedItem,
                                TransactionType = TransactionType.Undo
                            };
                        }
                        else
                        {
                            transactedItem = this.FirstOrDefault(x => x.ID == last_transaction.Item_ID);
                            if (transactedItem == null)
                            {
                                throw new DBCannotUndoException(string.Format("Failed to load item with ID {0} to reset {1}", last_transaction.Transacted_item.ID, last_transaction.changed_property));
                            }

                            transactedItem.PropertyChangedExtended -= this.DataBaseItem_PropertyChanged;

                            SetProperty(last_transaction, transactedItem);

                            undoTransaction = new DBTransaction<T>()
                            {
                                TransactionType = TransactionType.Undo,
                                Item_ID = last_transaction.Item_ID,
                                Transacted_item = null,
                                changed_property = last_transaction.changed_property,
                                property_new = last_transaction.property_old,
                                property_old = last_transaction.property_new
                            };
                        }
                    }
                    else
                    {
                        throw new DBCannotUndoException();
                    }

                    this.OnItemChanged(transactedItem.ID);
                    // store Undo transaction at start
                    last_transaction.Active = false;
                    this.Transactions_DB.Insert(0, undoTransaction);
                }
                finally
                {
                    // reregister changed handler
                    if (registerItem)
                    {
                        transactedItem.PropertyChangedExtended += this.DataBaseItem_PropertyChanged;
                    }

                    this.CollectionChanged += this.DataBase_CollectionChanged;

                    // update the DB on disk
                    _cacheDB();
                }
            }
            PublicOnPropertyChanged(nameof(CanUndo));
            PublicOnPropertyChanged(nameof(CanRedo));
        }

        public void Redo()
        {
            if (!this.CanRedo)
            {
                throw new DBCannotRedoException("Cannot redo at this time");
            }

            DBTransaction<T> last_transaction = GetLastTransaction(TransactionType.Redo, x => x.Active == true);
            T transactedItem;
            // get the mutex
            lock (Locker)
            {

                // deregister Changed handler

                this.CollectionChanged -= DataBase_CollectionChanged;

                if (last_transaction.TransactionType != TransactionType.Undo)
                {

                    throw new DBCannotRedoException();
                }

                DBTransaction<T> redoTransaction;
                if (last_transaction.changed_property == DBTransaction<T>.ITEM_ADDED)
                {
                    // get item from last transaction
                    transactedItem = this.FirstOrDefault(x => x.ID == last_transaction.Item_ID);
                    transactedItem.PropertyChangedExtended -= DataBaseItem_PropertyChanged;
                    if (transactedItem == null)
                    {
                        throw new DBCannotRedoException(string.Format("Failed to find item with ID {1} to redo property {0}", last_transaction.changed_property, last_transaction.Item_ID));
                    }

                    this.Remove(transactedItem);
                    redoTransaction = new DBTransaction<T>
                    {
                        changed_property = DBTransaction<T>.ITEM_REMOVED,
                        Transacted_item = transactedItem,
                        Item_ID = transactedItem.ID,
                        TransactionType = TransactionType.Redo
                    };
                }
                else if (last_transaction.changed_property == DBTransaction<T>.ITEM_REMOVED)
                {
                    transactedItem = last_transaction.Transacted_item;
                    transactedItem.PropertyChangedExtended -= DataBaseItem_PropertyChanged;
                    if (transactedItem == null)
                    {
                        throw new DBCannotRedoException(string.Format("Failed to load item with ID {0} to re-add", last_transaction.Transacted_item.ID));
                    }
                    this.Add(transactedItem);

                    redoTransaction = new DBTransaction<T>
                    {
                        changed_property = DBTransaction<T>.ITEM_ADDED,
                        Transacted_item = transactedItem,
                        Item_ID = transactedItem.ID,
                        TransactionType = TransactionType.Redo
                    };
                }
                else
                {
                    transactedItem = this.FirstOrDefault(x => x.ID == last_transaction.Item_ID);
                    transactedItem.PropertyChangedExtended -= DataBaseItem_PropertyChanged;
                    if (transactedItem == null)
                    {
                        throw new DBCannotRedoException(string.Format("Failed to load item with ID {0} to reset {1}", last_transaction.Transacted_item.ID, last_transaction.changed_property));
                    }

                    SetProperty(last_transaction, transactedItem);

                    redoTransaction = new DBTransaction<T>()
                    {
                        TransactionType = TransactionType.Redo,
                        Item_ID = last_transaction.Item_ID,
                        Transacted_item = null,
                        changed_property = last_transaction.changed_property,
                        property_new = last_transaction.property_old,
                        property_old = last_transaction.property_new
                    };
                }

                // store Redo transaction            
                Transactions_DB.Insert(0, redoTransaction);
                last_transaction.Active = false;
                // reregister changed handler
                transactedItem.PropertyChangedExtended += DataBaseItem_PropertyChanged;
                this.CollectionChanged += DataBase_CollectionChanged;

                _cacheDB();
            }
            this.OnItemChanged(transactedItem.ID);
            PublicOnPropertyChanged(nameof(CanRedo));
            PublicOnPropertyChanged(nameof(CanUndo));
            // update the DB
        }

        //public static DataBase<T> mergeDBs(DataBase<T> primary, DataBase<T> secondary, Func<DataBase<T>, DataBase<T>, bool> Conflict, Func<T, T, T> resolve)
        //{
        //    if (!Conflict(primary, secondary))
        //    {
        //        primary.CollectionChanged -= primary.DataBase_CollectionChanged;
        //        //foreach(T item in secondary)
        //        //{
        //        //    primary.Add(item);
        //        //    item.PropertyChangedExtended += primary.DataBase_PropertyChanged;
        //        //}
        //        primary.CollectionChanged += primary.DataBase_CollectionChanged;
        //    }

        //    return primary;
        //}

        public static bool conflict_if_same_item_changed_by_id(DataBase<T> db1, DataBase<T> db2)
        {
            bool result = false;
            // any transaction in db1 has the same item db as any tranasction in db2
            result = db1.Transactions_DB.Any(x => db2.Transactions_DB.Any(y => x.Item_ID == y.Item_ID));
            return result;
        }

        //private static void enactTransaction(DataBase<T> db, DBTransaction<T> transaction)
        //{
        //    db.CollectionChanged -= db.DataBase_CollectionChanged;
        //    DBTransaction<T> new_transaction;
        //    TransactionType transactionType;
        //    switch (transaction.TransactionType)
        //    {
        //        case (TransactionType.Add):
        //            var added_item = transaction.Transacted_item;
        //            db.Add(added_item);
        //            new_transaction = new DBTransaction<T>(transaction)
        //            {
        //                // ID may be updated to keep it unique in the db
        //                Item_ID = added_item.ID,
        //                Transacted_item = added_item
        //            };
        //            break;
        //        case (TransactionType.Delete):
        //            var transacted_item = db.FirstOrDefault(x => x.ID == transaction.Item_ID);
        //            if (transacted_item == null)
        //            {
        //                throw new DBException(string.Format("Cannot find item to remove with ID: {0}", transaction.Item_ID));
        //            }
        //            db.Remove(transacted_item);
        //            new_transaction = new DBTransaction<T>(transaction)
        //            {
        //                Transacted_item = transacted_item
        //            };
        //            break;
        //        case (TransactionType.Modify):
        //        case (TransactionType.Redo):
        //        case (TransactionType.Undo):
        //            var transactedItem = db.FirstOrDefault(x => x.ID == transaction.Item_ID);
        //            if (transactedItem == null)
        //            {
        //                throw new DBException(string.Format("Cannot find item to alter with ID: {0}", transaction.Item_ID));
        //            }

        //            PropertyInfo prop = transactedItem.GetType().GetProperty(transaction.changed_property, BindingFlags.Public | BindingFlags.Instance);
        //            if (prop != null && prop.CanWrite)
        //            {
        //                prop.SetValue(transactedItem, transaction.property_new, null);
        //            }
        //            else
        //            {
        //                throw new DBException(string.Format("Failed to set {0} property on item {1}", transaction.changed_property, transaction.Item_ID));
        //            }
        //            break;
        //        case (TransactionType.Unknown):
        //            throw new DBException("Don't know how to enact an unknown transaction");
        //    }

        //    db.CollectionChanged += db.DataBase_CollectionChanged;
        //}

        public delegate void TChangedEventHandler(object sender, ID id);
        public event TChangedEventHandler ItemChanged;
        #endregion

        #region private methods
        protected virtual string readFile(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                var json = System.IO.File.ReadAllText(filename);
                return json;
            }
            return "";
        }

        protected virtual DataBase<DBTransaction<T>> getTransactionsDB(string transactions_filename)
        {
            return new DataBase<DBTransaction<T>>(transactions_filename, this.DBVersion, this.MinimumCompatibleVersion, true);
        }

        private void LoadFile(string file, bool registerItemsForPropertyChange)
        {
            if (System.IO.File.Exists(file))
            {
                var json = readFile(file);
                if (json.Length > 0)
                {
                    var adapted = JsonConvert.DeserializeObject<DataBase<T>>(json, new DataBaseSerializer<T>());
                    if (adapted.DBVersion >= MinimumCompatibleVersion)
                    {
                        foreach (var item in adapted)
                        {
                            base.Add(item);
                            if (registerItemsForPropertyChange)
                                item.PropertyChangedExtended += DataBaseItem_PropertyChanged;
                        }
                    }
                    else
                    {
                        throw new Exception($"DB version {adapted.DBVersion} too old. Oldest supported db version is {MinimumCompatibleVersion}");
                    }
                }
            }
        }

        private static void SetProperty(DBTransaction<T> last_transaction, T transactedItem)
        {
            // redo with https://stackoverflow.com/a/13270302
            var properties = last_transaction.changed_property.Split('.');
            object lastObject = transactedItem;
            System.Reflection.PropertyInfo currentProperty = null;

            foreach (var attribute in properties)
            {
                // have currentObject lag behind since it should reflect the object the last property is on
                if (currentProperty != null)
                {
                    lastObject = currentProperty.GetValue(lastObject);
                }
                //get the property information based on the type
                if (!attribute.Contains("["))
                {
                    currentProperty = lastObject.GetType().GetProperty(attribute, BindingFlags.Public | BindingFlags.Instance);
                }
                else
                {
                    if (!attribute.Contains("]"))
                    {
                        throw new DBCannotUndoException($"Property name {attribute} contains unmatched '['");
                    }
                    if (attribute.IndexOf('[') != 0)
                    {
                        var propertyName = attribute.Substring(0, attribute.IndexOf('['));
                        currentProperty = lastObject.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance); // get the dictionary property
                        lastObject = currentProperty.GetValue(lastObject); // get the dictionary object
                        if (lastObject == null)
                        {
                            throw new DBCannotUndoException($"Cannot access property {propertyName} on {lastObject}");
                        }
                    }

                    Type t = lastObject.GetType();
                    if (!t.IsGenericType || t.GetGenericTypeDefinition() != typeof(Dictionary<,>))
                    {
                        throw new DBCannotUndoException($"Property {lastObject} is not a dictionary, but was used with indexers");
                    }
                    var r = new Regex(@"\[.+\]");
                    Match m = r.Match(attribute);
                    if (!m.Success)
                    {
                        // possible??
                        throw new DBCannotUndoException($"Cannot undo property: {attribute}");
                    }
                    var keyType = t.GetGenericArguments()[0];
                    var valueType = t.GetGenericArguments()[1];

                    // store key without square brackets
                    var key = m.Value.Substring(1);
                    key = key.Substring(0, key.Length - 1);
                    var keyObject = Convert.ChangeType(key, keyType);

                    //currentProperty = Convert.ChangeType(lastObject, keyType);
                    var p1 = t.GetProperty("Item"); // get indexer property
                    lastObject = p1.GetValue(lastObject, new object[] { keyObject });
                    currentProperty = null;
                }
            }

            if (currentProperty == null)
            {
                throw new DBCannotUndoException($"Cannot access property {properties.First()} on {lastObject}");
            }

            //find the property type
            Type propertyType = currentProperty.PropertyType;

            //Convert.ChangeType does not handle conversion to nullable types
            //if the property type is nullable, we need to get the underlying type of the property
            var targetType = IsNullableType(propertyType) ? Nullable.GetUnderlyingType(propertyType) : propertyType;

            //Returns an System.Object with the specified System.Type and whose value is
            //equivalent to the specified object.
            object oldVal = null;
            if (targetType.IsEnum)
            {
                // need converter for int to enum
                oldVal = Enum.ToObject(targetType, last_transaction.property_old);
            }
            else
            {
                oldVal = Convert.ChangeType(last_transaction.property_old, targetType);
            }


            //Set the value of the property
            currentProperty.SetValue(lastObject, oldVal, null);
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }

        private DBTransaction<T> GetLastTransaction(TransactionType notTransactionType, Func<DBTransaction<T>, bool> matcher)
        {
            DBTransaction<T> last_transaction = null;
            IEnumerable<DBTransaction<T>> temp_list = Transactions_DB;
            do
            {
                bool first_matches = false;
                // is first in list active?
                if (temp_list != null & temp_list.Count() > 0)
                {
                    first_matches = matcher(temp_list.First());
                }

                if (temp_list == null || temp_list.Count() == 0)
                {
                    // ran out of options
                    throw new DBCannotUndoException("Cannot find transaction");
                }
                if (temp_list.FirstOrDefault()?.TransactionType == notTransactionType)
                {
                    // transaction type to skip past
                    var count = this.countRecentTransactions(notTransactionType, temp_list);
                    temp_list = temp_list.Skip(count * 2);
                }
                else if (first_matches)
                {
                    last_transaction = temp_list.FirstOrDefault();
                }
                else
                {
                    temp_list = temp_list.Skip(1);
                }
            } while (last_transaction == null || last_transaction.TransactionType == notTransactionType);
            return last_transaction;
        }

        private DBTransaction<T> GetLastTransaction(List<TransactionType> notTransactionTypes)
        {
            DBTransaction<T> last_transaction = null;
            IEnumerable<DBTransaction<T>> temp_list = Transactions_DB;
            do
            {
                if (temp_list == null || temp_list.Count() == 0)
                {
                    throw new DBCannotUndoException("Cannot find transaction");
                }
                var first_transaction = temp_list.FirstOrDefault().TransactionType;
                if (notTransactionTypes.Contains(first_transaction))
                {
                    var count = this.countRecentTransactions(notTransactionTypes, temp_list);
                    temp_list = temp_list.Skip(count * 2);
                }
                else
                {
                    last_transaction = temp_list.FirstOrDefault();
                }
            } while (last_transaction == null || notTransactionTypes.Contains(last_transaction.TransactionType));
            return last_transaction;
        }

        public event PropertyChangedEventHandler PublicPropertyChanged;
        protected virtual void PublicOnPropertyChanged(string propertyName)
        {
            PublicPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int countRecentTransactions(TransactionType transactionType, IEnumerable<DBTransaction<T>> list = null)
        {
            if (list == null)
            {
                list = this.Transactions_DB;
            }
            var first = list.FirstOrDefault() as DBTransaction<T>;
            if (first == null)
            {
                return 0;
            }
            int count = (first.TransactionType == transactionType) ?
                list.Select((item, index) => new { item, index })
                    .Where(x => (x.item.TransactionType != transactionType)) // find non matches
                    .Select(x => x.index)                                    // select the index
                    .FirstOrDefault()                                        // return first index
                : 0;
            return count;
        }

        private int countRecentTransactions(Func<DBTransaction<T>, bool> predicate, IEnumerable<DBTransaction<T>> list = null)
        {
            if (list == null)
            {
                list = this.Transactions_DB;
            }
            var first = list.FirstOrDefault() as DBTransaction<T>;
            if (first == null)
            {
                return 0;
            }
            int count = (predicate(first)) ?
                list.Select((item, index) => new { item, index })
                    .Where(x => !predicate(x.item)) // find non matching
                    .Select(x => x.index)           // select its index
                    .FirstOrDefault()               // get first value (index of first non-match)
                : 0;
            return count;
        }

        private int countRecentTransactions(List<TransactionType> transactionTypes, IEnumerable<DBTransaction<T>> list = null)
        {
            if (list == null)
            {
                list = this.Transactions_DB;
            }
            var first = list.FirstOrDefault() as DBTransaction<T>;
            if (first == null)
            {
                return 0;
            }
            int count = (transactionTypes.Contains(first.TransactionType)) ?
                list.Select((item, index) => new { item, index })
                    .Where(x => !transactionTypes.Contains(x.item.TransactionType)).Select(x => x.index).FirstOrDefault()
                : 0;
            return count;
        }

        protected void DataBase_TransactionsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // called on primary db
            lock (Locker)
            {
                this.Transactions_DB._cacheDB();
            }
        }

        protected void DataBase_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            
            lock (Locker)
            {
                _cacheDB();
                // cache transactions
                TransactionType transactionType;
                DBTransaction<T> transaction;
                System.Collections.IList changed = null;
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    transactionType = TransactionType.Add;
                    foreach (T item in e.NewItems)
                    {
                        // register for property change
                        item.PropertyChangedExtended += DataBaseItem_PropertyChanged;
                        // create add transaction
                        transaction = new DBTransaction<T>()
                        {
                            TransactionType = transactionType,
                            Item_ID = (item as T).ID,
                            Transacted_item = (item as T)
                        };
                        Transactions_DB.Insert(0, transaction);
                    }
                    changed = e.NewItems;
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
                {
                    transactionType = TransactionType.Modify;
                    foreach (T item in e.NewItems)
                    {
                        // register for property change
                        item.PropertyChangedExtended += DataBaseItem_PropertyChanged;
                        // create modify transaction
                        transaction = new DBTransaction<T>()
                        {
                            TransactionType = transactionType,
                            Item_ID = (item as T).ID,
                            Transacted_item = (item as T)
                        };
                        Transactions_DB.Insert(0, transaction);
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    // removing the item - mark a delete transaction
                    transactionType = TransactionType.Delete;
                    foreach (var item in e.OldItems)
                    {
                        transaction = new DBTransaction<T>()
                        {
                            TransactionType = transactionType,
                            Item_ID = (item as T).ID,
                            Transacted_item = (item as T)
                        };
                        Transactions_DB.Insert(0, transaction);
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    // reset the entire collection
                    this.Transactions_DB.Clear();
                }
                else
                {
                    // not sure, but if anything was added or modified, we want to log it.
                    if (e.NewItems != null)
                    {
                        transactionType = TransactionType.Unknown;
                        foreach (var item in this)
                        {
                            transaction = new DBTransaction<T>()
                            {
                                TransactionType = transactionType,
                                Item_ID = (item as T).ID,
                                Transacted_item = (item as T)
                            };
                            Transactions_DB.Insert(0, transaction);
                        }
                    }
                }

            }
            PublicOnPropertyChanged(nameof(CanUndo));
            PublicOnPropertyChanged(nameof(CanRedo));
        }

        protected void DataBaseItem_PropertyChanged(object sender, PropertyChangedExtendedEventArgs e)
        {
            lock (Locker)
            {
                var item = sender as T;
                if (item == null)
                {
                    throw new Exception("Sender must be dbObject");
                }
                _cacheDB();
                // if not an undoable change, alert property changes and leave
                if (!e.UndoableChange)
                {
                    OnItemChanged(item);
                    PublicOnPropertyChanged(nameof(CanUndo));
                    PublicOnPropertyChanged(nameof(CanRedo));
                    return;
                }
                // else, store a modify with information about how to undo
                var transaction = new DBTransaction<T>()
                {
                    TransactionType = TransactionType.Modify,
                    Item_ID = item.ID,
                    Transacted_item = null,
                    property_old = e.OldValue,
                    property_new = e.NewValue,
                    changed_property = e.PropertyName
                };
                Transactions_DB.Insert(0, transaction);
                OnItemChanged(item);
                PublicOnPropertyChanged(nameof(CanUndo));
                PublicOnPropertyChanged(nameof(CanRedo));
            }
        }

        protected virtual void _cacheDB()
        {
            lock (Locker)
            {
                var json = serializeData;
                System.IO.File.WriteAllText(Filename, json);
            }
        }

        protected string serializeData
        {
            get
            {
                lock (Locker)
                {
                    // TODO: compress https://dotnet-snippets.de/snippet/strings-komprimieren-und-dekomprimieren/1058
                    return JsonConvert.SerializeObject(this, new DataBaseSerializer<T>());
                }
            }
        }

        protected virtual void OnItemChanged(T itemChanged)
        {
            ItemChanged?.Invoke(this, itemChanged?.ID);
        }
        protected virtual void OnItemChanged(ID id)
        {
            ItemChanged?.Invoke(this, id);
        }
        #endregion
    }
}
