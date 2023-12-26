using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLModel
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyAttribute : FieldAttribute
    {
        public string ReferenceTableName { get; }
        public string ReferenceFieldName { get; }
        public Type ReferenceClass { get; }

        public ForeignKeyAttribute(string columnName, string columnType, string reference, Type referenceClass) : 
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
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid ForeignKeyAttribute", ex);
            }
        }
    }
}
