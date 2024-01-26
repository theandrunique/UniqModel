using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UniqModel
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
            foreach (var foreignKey in table.ForeignKeys)
            {
                if (!createdTables.Contains(foreignKey.ReferenceTableName))
                {
                    return false;
                }
            }
            return true;
        }
    }
}