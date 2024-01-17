using System;

namespace SQLModel
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FieldAttribute : Attribute
    {
        public string ColumnName { get { return columnName; } }
        private string columnName;
        public string ColumnType { get { return columnType; } }
        private string columnType;
        public FieldAttribute() { }
        public FieldAttribute(string columnName)
        {
            this.columnName = columnName;
        }
        public FieldAttribute(string columnName, string columnType)
            : this(columnName)
        {
            this.columnType = columnType;
        }
    }
}
