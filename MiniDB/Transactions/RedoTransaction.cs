using System;
using System.Collections.Generic;
using System.Linq;

using MiniDB.Interfaces;

namespace MiniDB.Transactions
{
    public class RedoTransaction : BaseDBTransaction, IModifyTransaction, IWholeItemTransaction
    {
        public RedoTransaction(IDBObject transactedObject, DBTransactionType subTransactionType) : base(transactedObject.ID)
        {
            this.SubDBTransactionType = subTransactionType;
            this.TransactedItem = transactedObject;
        }

        public RedoTransaction(ID changedItemID, string changedPropertyName, object oldValue, object newValue) : base(changedItemID)
        {
            this.SubDBTransactionType = DBTransactionType.Modify;
            this.ChangedFieldName = changedPropertyName;
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }

        public RedoTransaction(IDBTransaction other) : base(other)
        {
            if (other.DBTransactionType != this.DBTransactionType)
            {
                throw new DBException($"Attempted to create class of type {nameof(AddTransaction)}, but parameter used of type {other.DBTransactionType}");
            }
        }

        public override DBTransactionType DBTransactionType => DBTransactionType.Redo;

        public DBTransactionType SubDBTransactionType { get; }

        public IDBObject TransactedItem { get; }

        public string ChangedFieldName { get; }

        public object OldValue { get; }

        public object NewValue { get; }

        public override IDBTransaction Revert(IList<IDBObject> objects, PropertyChangedExtendedEventHandler notifier)
        {
            IDBTransaction result = null;

            if (this.SubDBTransactionType == DBTransactionType.Add)
            {
                // revert an add
                if (this.TransactedItem == null)
                {
                    throw new DBCannotUndoException($"Cannot find item to re-remove");
                }
                var transactedItem = objects.FirstOrDefault(dbItem => dbItem.ID == this.TransactedItem.ID);
                if (transactedItem == null)
                {
                    throw new DBCannotUndoException($"Cannot find item with ID: {this.TransactedItem.ID}");
                }

                transactedItem.PropertyChangedExtended -= notifier;

                objects.Remove(transactedItem);

                result = new UndoTransaction(transactedItem, DBTransactionType.Add);
            }
            else if (this.SubDBTransactionType == DBTransactionType.Modify)
            {
                // undo a modify
                if (this.ChangedItemID == null)
                {
                    throw new DBCannotUndoException("Error with stored undo command. No ID stored.");
                }

                var transactedItem = objects.FirstOrDefault(item => item.ID == this.ChangedItemID);
                if (transactedItem == null)
                {
                    throw new DBCannotUndoException($"Cannot find item with ID {this.ChangedItemID} to change");
                }

                ModifyTransactionHelpers.ExecuteInTransactionBlockingScope(notifier, transactedItem, this, ModifyTransactionHelpers.RevertProperty);
                result = new UndoTransaction(
                    changedItemID: this.ChangedItemID,
                    changedPropertyName: this.ChangedFieldName,
                    newValue: this.OldValue,
                    oldValue: this.NewValue);
            }
            else
            {
                throw new NotImplementedException("TODO: implement rest of revert undo");
            }


            if (result == null)
            {
                throw new DBCannotRedoException($"Failure attempting to revert Redo procedure: {this}");
            }

            this.Active = false;
            return result;
        }
    }
}
