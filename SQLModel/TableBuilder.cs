using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Windows.Documents;

namespace SQLModel
{
    internal class TableBuilder
    {
        public static void CreateTable(Table table, Session session)
        {
            string createTableQuery = TableBuilder.GenerateCreateTableQuery(table, session);
            session.ExecuteNonQuery(createTableQuery);
        }
        public static void CreateForeignKeys(Type type, Core dbcore)
        {
            if (dbcore.DatabaseProvider is SqliteDatabaseProvider)
            {
                throw new NotSupportedException("SqliteDatabaseProvider does not support creating foreign keys after creating tables");
            }

            PropertyInfo[] properties = type.GetProperties();
            
            foreach(PropertyInfo property in properties)
            {
                var foreignKeyAttribute = (ForeignKeyAttribute)property.GetCustomAttribute(typeof(ForeignKeyAttribute));
                if (foreignKeyAttribute != null)
                {
                    string query = GenerateAddForeignKeyQuery(type, foreignKeyAttribute);
                    using (var session = dbcore.CreateSession())
                    {
                        session.ExecuteNonQuery(query);
                    }
                }
            }
        }
        private static string GenerateCreateTableQuery(Table table, Session session)
        {
            string createTableQuery = $"CREATE TABLE {table.Name} (";

            foreach (var item in table.FieldsRelation.Keys)
            {
                var field = table.FieldsRelation[item];

                if (table.PrimaryKeys.Count < 2 && field.PrimaryKey)
                {
                    createTableQuery += $"{field.Name} {field.Type} {session.DbCore.DatabaseProvider.GetAutoIncrementWithType()}, ";
                } else
                {
                    createTableQuery += $"{field.Name} {field.Type}, ";
                }
            }

            foreach (var foreignKey in table.ForeignKeys)
            {
                createTableQuery += $"{ForeignKeyWithQuery(foreignKey)}, ";
            }

            if (table.PrimaryKeys.Count > 2)
            {
                createTableQuery += "PRIMARY KEY (" + string.Join(", ", table.PrimaryKeys.Select(key => $"{key.ColumnName}")) + ")";
            }

            createTableQuery = createTableQuery.TrimEnd(',', ' ') + ");";
            return createTableQuery;
        }
        private static string ForeignKeyWithQuery(ForeignKeyAttribute foreignKey)
        {
            return $"FOREIGN KEY ({foreignKey.ColumnName}) REFERENCES {foreignKey.ReferenceTableName} ({foreignKey.ReferenceFieldName}) ON DELETE {foreignKey.OnDeleteRule} ON UPDATE {foreignKey.OnUpdateRule}";
        }
        private static string GenerateAddForeignKeyQuery(Type type, ForeignKeyAttribute foreignKeyAttribute)
        {
            var tableAttribute = (TableAttribute)type.GetCustomAttribute(typeof(TableAttribute));

            return $"ALTER TABLE {tableAttribute.TableName} " +
                $"ADD CONSTRAINT FK_{tableAttribute.TableName}_{foreignKeyAttribute.ReferenceTableName} " +
                $"FOREIGN KEY ({foreignKeyAttribute.ColumnName}) " +
                $"REFERENCES {foreignKeyAttribute.ReferenceTableName} ({foreignKeyAttribute.ReferenceFieldName}) " +
                $"ON DELETE {foreignKeyAttribute.OnDeleteRule} " +
                $"ON UPDATE {foreignKeyAttribute.OnUpdateRule};";
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
