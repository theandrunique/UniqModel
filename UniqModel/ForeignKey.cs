using System.Reflection;

namespace UniqModel
{
    public class ForeignKey : Field
    {
        public string ReferenceTableName { get; }
        public string ReferenceFieldName { get; }
        public string OnDeleteRule { get; }
        public string OnUpdateRule { get; }
        public ForeignKey(ForeignKeyAttribute attribute, PropertyInfo property)
            : base(attribute, property, false, true)
        {
            ReferenceTableName = attribute.ReferenceTableName;
            ReferenceFieldName = attribute.ReferenceFieldName;
            OnDeleteRule = attribute.OnDeleteRule;
            OnUpdateRule = attribute.OnUpdateRule;
        }
    }
}