using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MiniDB.Transactions
{
    internal class ModifyTransactionHelpers
    {
        public static void ExecuteInTransactionBlockingScope(PropertyChangedExtendedEventHandler notifier, IDBObject item, IModifyTransaction transaction, Action<IModifyTransaction, IDBObject> action)
        {
            using (new TransactionBlockScope(item, notifier))
            {
                action.Invoke(transaction, item);
            }
        }

        /// <summary>
        /// Use reflection to set reverse the last transaction on the transacted item
        /// </summary>
        /// <param name="last_transaction">what happened that we're changing back</param>
        /// <param name="transactedItem">the item to act on</param>
        public static void RevertProperty(IModifyTransaction last_transaction, IDBObject transactedItem)
        {
            // TODO: redo with https://stackoverflow.com/a/13270302
            var properties = last_transaction.ChangedFieldName.Split('.');
            object lastObject = transactedItem;
            System.Reflection.PropertyInfo currentProperty = null;

            foreach (var attribute in properties)
            {
                // have currentObject lag behind since it should reflect the object the last property is on
                if (currentProperty != null)
                {
                    lastObject = currentProperty.GetValue(lastObject);
                }

                // get the property information based on the type
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

                    // currentProperty = Convert.ChangeType(lastObject, keyType);
                    var p1 = t.GetProperty("Item"); // get indexer property
                    lastObject = p1.GetValue(lastObject, new object[] { keyObject });
                    currentProperty = null;
                }
            }

            if (currentProperty == null)
            {
                throw new DBCannotUndoException($"Cannot access property {properties.First()} on {lastObject}");
            }

            // find the property type
            Type propertyType = currentProperty.PropertyType;

            // Convert.ChangeType does not handle conversion to nullable types
            //  if the property type is nullable, we need to get the underlying type of the property
            var targetType = IsNullableType(propertyType) ? Nullable.GetUnderlyingType(propertyType) : propertyType;

            // Returns an System.Object with the specified System.Type and whose value is
            //   equivalent to the specified object.
            object oldVal = null;
            if (targetType.IsEnum)
            {
                // need converter for int to enum
                oldVal = Enum.ToObject(targetType, last_transaction.OldValue);
            }
            else
            {
                oldVal = Convert.ChangeType(last_transaction.OldValue, targetType);
            }

            // Set the value of the property
            currentProperty.SetValue(lastObject, oldVal, null);
        }

        /// <summary>
        /// Determine if the objece is a nullable type
        /// </summary>
        /// <param name="type">the type in question</param>
        /// <returns>true if nullable</returns>
        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }

        private class TransactionBlockScope : IDisposable
        {
            private readonly IDBObject item;

            private readonly PropertyChangedExtendedEventHandler dataChangedHandler;

            public TransactionBlockScope(IDBObject item, PropertyChangedExtendedEventHandler dataChangedHandler)
            {
                this.item = item;
                this.dataChangedHandler = dataChangedHandler;

                this.item.PropertyChangedExtended -= this.dataChangedHandler;
            }

            public void Dispose()
            {
                this.item.PropertyChangedExtended += this.dataChangedHandler;
            }
        }
    }
}
