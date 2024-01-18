using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SQLModel
{
    public class Metadata
    {
        private Core dbcore;
        public static Dictionary<Type, Table> TableClasses = new Dictionary<Type, Table>();
        public Metadata(Core dbcore)
        {
            Init();
            this.dbcore = dbcore;
        }
        private void Init()
        {
            List<Type> tables = GetBaseModelTypes();
            foreach (Type table in tables)
            {
                Table t = new Table(table);
                TableClasses[table] = t;
            }

            CheckForeignKeysSyntax();
        }
        private void CheckForeignKeysSyntax()
        {
            Dictionary<string, Table> tablesNames = new Dictionary<string, Table>();

            foreach (Table table in TableClasses.Values.ToList())
            {
                tablesNames[table.Name] = table;
            }

            foreach (Type tableClass in TableClasses.Keys.ToList())
            {
                Table table = TableClasses[tableClass];

                foreach (ForeignKey foreignKey in table.ForeignKeys)
                {
                    if (!tablesNames.ContainsKey(foreignKey.ReferenceTableName))
                    {
                        throw new ArgumentException($"Foreign key reference table '{foreignKey.ReferenceTableName}' not found in the existing tables. Details: class '{tableClass.Name}'");
                    }
                    Table referencedTable = tablesNames[foreignKey.ReferenceTableName];

                    List<string> fields = referencedTable.FieldsRelation.Values.Select(f => f.Name).ToList();
                    if (!fields.Contains(foreignKey.ReferenceFieldName))
                    {
                        throw new ArgumentException($"Field '{foreignKey.ReferenceFieldName}' not found in the referenced table '{foreignKey.ReferenceTableName}'. Details: class '{tableClass.Name}', foreign key property name: '{foreignKey.Property.Name}'");
                    }
                    if (!referencedTable.PrimaryKeys.Select(pk => pk.Name).ToList().Contains(foreignKey.ReferenceFieldName))
                    {
                        Logging.Warning($"Foreign key '{foreignKey.Name}' in table '{table.Name}' references a non-primary key field in the table '{foreignKey.ReferenceTableName}'. Details: class '{tableClass.Name}', foreign key property name: '{foreignKey.Property.Name}'");
                    }
                }
            }
        }
        private static List<Type> GetBaseModelTypes()
        {
            List<Type> typesList = new List<Type>();

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var types = asm.GetTypes().Where(type => type.BaseType == typeof(BaseModel));
                typesList.AddRange(types);
            }
            return typesList;
        }
        public void CreateAll()
        {
            HashSet<string> createdTables = new HashSet<string>();

            List<Table> tablesToCreate = TableClasses.Values.ToList();

            while (tablesToCreate.Count != 0)
            {
                foreach (Table table in tablesToCreate.ToList())
                {
                    if (dbcore.TableExists(table.Name))
                    {
                        createdTables.Add(table.Name);
                        tablesToCreate.Remove(table);
                        continue;
                    }
                    if (CanBeCreated(table, createdTables))
                    {
                        using (var session = dbcore.CreateSession())
                        {
                            TableBuilder.CreateTable(table, session);
                        }
                        createdTables.Add(table.Name);
                        tablesToCreate.Remove(table);
                    }
                }
            }
        }
        private bool CanBeCreated(Table table, HashSet<string> createdTables)
        {
            foreach(var foreignKey in table.ForeignKeys)
            {
                if (!createdTables.Contains(foreignKey.ReferenceTableName))
                {
                    return false;
                }
            }
            return true;
        }
    }
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
                Type = Core.GetSqlType(property.PropertyType);
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
    public class PrimaryKey : Field
    {
        public PrimaryKey(FieldAttribute attribute, PropertyInfo property)
            : base(attribute, property, true, false) { }
    }
}