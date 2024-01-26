using System;
using System.Linq;
using System.Reflection;

namespace UniqModel
{
    internal class TableBuilder
    {
        public static void CreateTable(Table table, Session session)
        {
            string createTableQuery = GenerateCreateTableQuery(table, session);
            session.ExecuteNonQuery(createTableQuery);
        }
        public static void CreateForeignKeys(Type typeTable, Core dbcore)
        {
            //if (dbcore.DatabaseProvider is SqliteDatabaseProvider)
            //{
            //    throw new NotSupportedException("SqliteDatabaseProvider does not support creating foreign keys after creating tables");
            //}

            Table table = Metadata.TableClasses[typeTable];

            foreach (ForeignKey key in table.ForeignKeys)
            {
                string query = GenerateAddForeignKeyQuery(table.Name, key);

                using (var session = dbcore.CreateSession())
                {
                    session.ExecuteNonQuery(query);
                }
            }
        }
        private static string GenerateCreateTableQuery(Table table, Session session)
        {
            string createTableQuery = $"CREATE TABLE {table.Name} (";

            foreach (PropertyInfo item in table.FieldsRelation.Keys)
            {
                Field field = table.FieldsRelation[item];

                if (table.PrimaryKeys.Count < 2 && field.PrimaryKey)
                {
                    createTableQuery += $"{field.Name} {session.DbCore.DatabaseProvider.GetAutoIncrementWithType()}, ";
                }
                else
                {
                    createTableQuery += $"{field.Name} {field.Type}, ";
                }
            }

            foreach (ForeignKey foreignKey in table.ForeignKeys)
            {
                createTableQuery += $"{ForeignKeyWithQuery(foreignKey)}, ";
            }

            if (table.PrimaryKeys.Count > 2)
            {
                createTableQuery += "PRIMARY KEY (" + string.Join(", ", table.PrimaryKeys.Select(key => $"{key.Name}")) + ")";
            }

            createTableQuery = createTableQuery.TrimEnd(',', ' ') + ");";
            return createTableQuery;
        }
        private static string ForeignKeyWithQuery(ForeignKey foreignKey)
        {
            return $"FOREIGN KEY ({foreignKey.Name}) REFERENCES {foreignKey.ReferenceTableName} ({foreignKey.ReferenceFieldName}) ON DELETE {foreignKey.OnDeleteRule} ON UPDATE {foreignKey.OnUpdateRule}";
        }
        private static string GenerateAddForeignKeyQuery(string tableName, ForeignKey foreignKey)
        {
            return $"ALTER TABLE {tableName} " +
                $"ADD CONSTRAINT FK_{tableName}_{foreignKey.ReferenceTableName} " +
                $"FOREIGN KEY ({foreignKey.Name}) " +
                $"REFERENCES {foreignKey.ReferenceTableName} ({foreignKey.ReferenceFieldName}) " +
                $"ON DELETE {foreignKey.OnDeleteRule} " +
                $"ON UPDATE {foreignKey.OnUpdateRule};";
        }
        //private static string GetSqlType(Type propertyType)
        //{
        //    if (propertyType == typeof(int))
        //    {
        //        return "INT";
        //    }
        //    else if (propertyType == typeof(string))
        //    {
        //        return "NVARCHAR(MAX)";
        //    }

        //    throw new NotSupportedException($"Type {propertyType.Name} is not supported.");
        //}
    }
}
