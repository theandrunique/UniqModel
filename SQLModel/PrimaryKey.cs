using System.Reflection;

namespace SQLModel
{
    public class PrimaryKey : Field
    {
        public PrimaryKey(FieldAttribute attribute, PropertyInfo property)
            : base(attribute, property, true, false) { }
    }
}