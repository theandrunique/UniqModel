using System;
using System.IO;

namespace SQLModel
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public string TableName { get; }

        public TableAttribute(string tableName)
        {
            TableName = tableName;
        }
    }
}
