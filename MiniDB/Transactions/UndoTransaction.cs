using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniDB.Transactions
{
    public class UndoTransaction : BaseDBTransaction, IWholeItemTransaction, IModifyTransaction
    {
        public UndoTransaction(IDBObject transactedItem, DBTransactionType subTransactionType) : base(transactedItem.ID)
        {
            this.TransactedItem = transactedItem;
            this.SubDBTransactionType = subTransactionType;
        }

        public UndoTransaction(ID changedItemID, string changedPropertyName, object oldValue, object newValue) : base(changedItemID)
        {
            this.SubDBTransactionType = DBTransactionType.Modify;
            this.ChangedFieldName = changedPropertyName;
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }

        public UndoTransaction(IDBTransaction other) : base(other)
        {
            if (other.DBTransactionType != this.DBTransactionType)
            {
                throw new DBException($"Attempted to create class of type {nameof(AddTransaction)}, but parameter used of type {other.DBTransactionType}");
            }
        }

        public override DBTransactionType DBTransactionType => DBTransactionType.Undo;

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
                // redo an add
                if (this.TransactedItem == null)
                {
                    throw new DBCannotRedoException($"Cannot find item to re-add");
                }

                var transactedItem = this.TransactedItem;
                transactedItem.PropertyChangedExtended += notifier;

                objects.Add(transactedItem);

                result = new RedoTransaction(transactedItem, DBTransactionType.Add);
            }
            else if (this.SubDBTransactionType == DBTransactionType.Modify)
            {
                // redo a modify
                if (this.ChangedItemID == null)
                {
                    throw new DBCannotRedoException($"Cannot find item to re-modify");
                }

                var transactedItem = objects.FirstOrDefault(item => item.ID == this.ChangedItemID);

                ModifyTransactionHelpers.ExecuteInTransactionBlockingScope(notifier, transactedItem, this, ModifyTransactionHelpers.RevertProperty);
                result = new RedoTransaction(
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
                throw new DBCannotRedoException($"Failure attempting to revert Undo proocedure: {this}");
            }

            this.Active = false;
            return result;
        }
    }
}
