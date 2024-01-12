using System;

namespace SQLModel
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyAttribute : FieldAttribute
    {
        public string ReferenceTableName { get; }
        public string ReferenceFieldName { get; }
        public Type ReferenceClass { get; }
        public string OnDeleteRule { get; }
        public string OnUpdateRule { get; }
        public ForeignKeyAttribute(string columnName, string columnType, string reference, Type referenceClass, 
            ForeignKeyRule onDeleteRule = ForeignKeyRule.Restrict, ForeignKeyRule onUpdateRule = ForeignKeyRule.Restrict) : 
            base(columnName, columnType)
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
                ReferenceClass = referenceClass;
                OnDeleteRule = ForeignKeyRuleMapper.MapToSql(onDeleteRule);
                OnUpdateRule = ForeignKeyRuleMapper.MapToSql(onUpdateRule);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid ForeignKeyAttribute", ex);
            }
        }
    }
}
