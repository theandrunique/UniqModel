using System;

namespace SQLModel
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FieldAttribute : Attribute
    {
        public string ColumnName { get; }
        public string ColumnType { get; }
        public bool IsPrimaryKey { get; }
        public bool IsForeignKey { get; }
        public FieldAttribute(string columnName, string columnType)
        {
            ColumnName = columnName;
            ColumnType = columnType;
        }
        public FieldAttribute(string columnName, string columnType, bool isPrimaryKey, bool isForeignKey)
            : this(columnName, columnType)
        {
            IsForeignKey = isForeignKey;
            IsPrimaryKey = isPrimaryKey;
        }
    }
}
