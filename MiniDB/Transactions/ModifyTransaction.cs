using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiniDB.Transactions
{
    public class ModifyTransaction : BaseDBTransaction, IModifyTransaction
    {
        public ModifyTransaction(ID changedItemID, string fieldName, object oldValue, object newValue) : base(changedItemID)
        {
            this.ChangedFieldName = fieldName;
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }

        public ModifyTransaction(IModifyTransaction other) : base(other)
        {
            this.ChangedFieldName = other.ChangedFieldName;
            this.OldValue = other.OldValue;
            this.NewValue = other.NewValue;
        }

        public override DBTransactionType DBTransactionType => DBTransactionType.Modify;

        public string ChangedFieldName { get; }

        public object OldValue { get; }

        public object NewValue { get; }

        public override IDBTransaction revert(IList<IDBObject> objects, PropertyChangedExtendedEventHandler notifier)
        {
            var transactedItem = objects.FirstOrDefault(item => item.ID == this.ChangedItemID);
            if(transactedItem == null)
            {
                throw new DBCannotUndoException($"Failed to find item with with ID {this.ChangedItemID} to undo property {this.ChangedFieldName}");
            }

            ModifyTransactionHelpers.ExecuteInTransactionBlockingScope(notifier, transactedItem, this, ModifyTransactionHelpers.RevertProperty);
                
            var result = new UndoTransaction(
                changedItemID: this.ChangedItemID,
                changedPropertyName: this.ChangedFieldName,
                oldValue: this.NewValue,
                newValue: this.OldValue);
            return result;
        }

        
    }
}
