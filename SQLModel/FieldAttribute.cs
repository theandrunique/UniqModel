using System;

namespace SQLModel
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FieldAttribute : Attribute
    {
        public string ColumnName { get; }
        public string ColumnType { get; }

        public FieldAttribute(string columnName, string columnType)
        {
            ColumnName = columnName;
            ColumnType = columnType;
        }
    }
}
