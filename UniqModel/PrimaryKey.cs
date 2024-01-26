using System.Reflection;

namespace UniqModel
{
    public class PrimaryKey : Field
    {
        public PrimaryKey(FieldAttribute attribute, PropertyInfo property)
            : base(attribute, property, true, false) { }
    }
}