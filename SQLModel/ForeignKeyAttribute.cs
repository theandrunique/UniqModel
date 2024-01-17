using System;

namespace SQLModel
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyAttribute : FieldAttribute
    {
        public string ReferenceTableName { get; }
        public string ReferenceFieldName { get; }
        public string OnDeleteRule { get; }
        public string OnUpdateRule { get; }

        public ForeignKeyAttribute(string columnName, string reference)
            : this(columnName, reference, null)
        {

        }
        public ForeignKeyAttribute(string columnName, string reference, string columnType)
            : base(columnName, columnType)
        {
            try
            {
                string[] arr = reference.Split('.');

                if (arr.Length != 2)
                {
                    throw new ArgumentException("Invalid format for reference. It should be in the format 'TableName.FieldName'.");
                }
                ReferenceTableName = arr[0];
                ReferenceFieldName = arr[1];
                OnDeleteRule = ForeignKeyRuleMapper.MapToSql(ForeignKeyRule.Restrict);
                OnUpdateRule = ForeignKeyRuleMapper.MapToSql(ForeignKeyRule.Restrict);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid ForeignKeyAttribute", ex);
            }
        }
        public ForeignKeyAttribute(string columnName, string reference, string columnType, ForeignKeyRule onDeleteRule)
            : this(columnName, reference, columnType)
        {
            OnDeleteRule = ForeignKeyRuleMapper.MapToSql(onDeleteRule);
        }
        public ForeignKeyAttribute(string columnName, string reference, string columnType, ForeignKeyRule onDeleteRule, ForeignKeyRule onUpdateRule)
            : this(columnName, reference, columnType)
        {
            OnDeleteRule = ForeignKeyRuleMapper.MapToSql(onDeleteRule);
            OnUpdateRule = ForeignKeyRuleMapper.MapToSql(onUpdateRule);
        }
    }
}
