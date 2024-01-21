using System;
using System.Collections.Generic;
using System.Reflection;

namespace SQLModel
{
    public class Table
    {
        public string Name { get; set; }
        public List<PrimaryKey> PrimaryKeys = new List<PrimaryKey>();
        public List<ForeignKey> ForeignKeys = new List<ForeignKey>();
        public Dictionary<PropertyInfo, Field> FieldsRelation = new Dictionary<PropertyInfo, Field>();
        public Table(Type table)
        {
            var tablenameValue = table.GetField("Tablename", BindingFlags.Public | BindingFlags.Static);

            if (tablenameValue == null)
            {
                Name = table.Name;
            } else {
                Name = (string)tablenameValue.GetValue(null);
            }

            GetFieldsProperties(table);
        }
        private void GetFieldsProperties(Type table)
        {
            PropertyInfo[] properties = table.GetProperties();

            foreach (var property in properties)
            {
                var fieldAttribute = (FieldAttribute)property.GetCustomAttribute(typeof(FieldAttribute));

                if (fieldAttribute != null)
                {
                    if (fieldAttribute is PrimaryKeyAttribute)
                    {
                        PrimaryKeyAttribute primaryKeyAttribute = (PrimaryKeyAttribute)property.GetCustomAttribute(typeof(PrimaryKeyAttribute));

                        PrimaryKey primaryKey = new PrimaryKey(primaryKeyAttribute, property);

                        PrimaryKeys.Add(primaryKey);

                        FieldsRelation[property] = primaryKey;
                    }
                    else if (fieldAttribute is ForeignKeyAttribute)
                    {
                        ForeignKeyAttribute foreignKeyAttribute = (ForeignKeyAttribute)property.GetCustomAttribute(typeof(ForeignKeyAttribute));

                        ForeignKey newForeignKey = new ForeignKey(foreignKeyAttribute, property);

                        ForeignKeys.Add(newForeignKey);

                        FieldsRelation[property] = newForeignKey; 

                    } else if (fieldAttribute is FieldAttribute)
                    {
                        FieldsRelation[property] = new Field(fieldAttribute, property);
                    }
                }
            }
        }
    }
}