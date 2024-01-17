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

        public ForeignKeyAttribute(string reference) : this(reference, null, null) { }
        public ForeignKeyAttribute(string reference, string columnName)
            : this(reference, columnName, null) { }
        public ForeignKeyAttribute(string reference, string columnName, string columnType)
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
        public ForeignKeyAttribute(string reference, string columnName, string columnType, ForeignKeyRule onDeleteRule)
            : this(reference, columnName, columnType)
        {
            OnDeleteRule = ForeignKeyRuleMapper.MapToSql(onDeleteRule);
        }
        public ForeignKeyAttribute(string reference, string columnName, string columnType, ForeignKeyRule onDeleteRule, ForeignKeyRule onUpdateRule)
            : this(reference, columnName, columnType)
        {
            OnDeleteRule = ForeignKeyRuleMapper.MapToSql(onDeleteRule);
            OnUpdateRule = ForeignKeyRuleMapper.MapToSql(onUpdateRule);
        }
    }
}
