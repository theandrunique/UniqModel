using System;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Reflection;

namespace SQLModel
{
    internal class TableBuilder
    {
        public static void CreateTable(Type type, Session session)
        {
            string createTableQuery = TableBuilder.GenerateCreateTableQuery(type, session);

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
        private static string GenerateCreateTableQuery(Type type, Session session)
        {
            var tableAttribute = (TableAttribute)type.GetCustomAttribute(typeof(TableAttribute));

            string createTableQuery = $"CREATE TABLE {tableAttribute.TableName} (";

            PropertyInfo[] properties = type.GetProperties();

            foreach (var property in properties)
            {
                var fieldAttribute = (FieldAttribute)property.GetCustomAttribute(typeof(FieldAttribute));

                if (fieldAttribute is PrimaryKeyAttribute)
                {
                    createTableQuery += $"{fieldAttribute.ColumnName} {fieldAttribute.ColumnType} {session.DbCore.DatabaseProvider.GetAutoIncrementWithType()}, ";
                } 
                else if (fieldAttribute is FieldAttribute)
                {
                    createTableQuery += $"{fieldAttribute.ColumnName} {fieldAttribute.ColumnType}, ";
                }
                else if (fieldAttribute is ForeignKeyAttribute)
                {
                    createTableQuery += $"{fieldAttribute.ColumnName} {fieldAttribute.ColumnType}, ";
                }
            }
            createTableQuery = createTableQuery.TrimEnd(',', ' ') + ");";
            return createTableQuery;
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
