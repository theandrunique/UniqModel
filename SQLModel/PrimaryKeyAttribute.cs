using System;

namespace SQLModel
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : FieldAttribute
    {
        public PrimaryKeyAttribute(string columnName) : base(columnName, null)
        {
        }
    }
}
