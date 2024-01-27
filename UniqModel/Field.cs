using System.Reflection;

namespace UniqModel
{
    public class Field
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool PrimaryKey { get; set; }
        public bool ForeignKey { get; set; }
        public PropertyInfo Property { get; set; }
        public Field(FieldAttribute attribute, PropertyInfo property)
        {
            if (attribute.ColumnName == null)
            {
                Name = property.Name;
            }
            else
            {
                Name = attribute.ColumnName;
            }

            if (attribute.ColumnType == null)
            {
                Type = CoreImpl.GetSqlType(property.PropertyType);
            }
            else
            {
                Type = attribute.ColumnType;
            }

            Property = property;
            PrimaryKey = false;
            ForeignKey = false;
        }
        public Field(FieldAttribute attribute, PropertyInfo property, bool isPrimaryKey, bool isForeignKey)
            : this(attribute, property)
        {
            PrimaryKey = isPrimaryKey;
            ForeignKey = isForeignKey;
        }
    }
}