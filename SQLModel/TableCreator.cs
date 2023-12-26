using System;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Reflection;

namespace SQLModel
{
    public class TableCreator
    {
        public static void CreateTable(Type type, SessionMaker session)
        {
            string createTableQuery = TableCreator.GenerateCreateTableQuery(type);

            session.Execute(createTableQuery);
        }
        public static void CreateForeignKey(Type type, SessionMaker session)
        {
            PropertyInfo[] properties = type.GetProperties();
            
            foreach(PropertyInfo property in properties)
            {
                var foreignKeyAttribute = (ForeignKeyAttribute)property.GetCustomAttribute(typeof(ForeignKeyAttribute));
                if (foreignKeyAttribute != null)
                {
                    string query = GenerateAddForeignKeyQuery(type, property, foreignKeyAttribute);
                    session.Execute(query);
                }
            }
        }
        private static string GenerateCreateTableQuery(Type type)
        {
            var tableAttribute = (TableAttribute)type.GetCustomAttribute(typeof(TableAttribute));

            if (tableAttribute == null)
            {
                throw new ArgumentException("The class must be marked with TableAttribute.");
            }

            string createTableQuery = $"CREATE TABLE IF NOT EXISTS {tableAttribute.TableName} (";

            PropertyInfo[] properties = type.GetProperties();

            foreach (var property in properties)
            {
                var fieldAttribute = (FieldAttribute)property.GetCustomAttribute(typeof(FieldAttribute));

                if (fieldAttribute is PrimaryKeyAttribute)
                {
                    createTableQuery += $"{fieldAttribute.ColumnName} {fieldAttribute.ColumnType} IDENTITY(1,1) PRIMARY KEY, ";
                } else
                if (fieldAttribute is FieldAttribute)
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

        private static string GenerateAddForeignKeyQuery(Type type, PropertyInfo property, ForeignKeyAttribute foreignKeyAttribute)
        {
            var tableAttribute = (TableAttribute)type.GetCustomAttribute(typeof(TableAttribute));

            if (tableAttribute == null)
            {
                throw new ArgumentException("The class must be marked with TableAttribute.");
            }

            return $"ALTER TABLE {tableAttribute.TableName} " +
                $"ADD CONSTRAINT FK_{tableAttribute.TableName}_{foreignKeyAttribute.ReferenceTableName} " +
                $"FOREIGN KEY ({foreignKeyAttribute.ColumnName}) " +
                $"REFERENCES {tableAttribute.TableName} ( {foreignKeyAttribute.ReferenceTableName});";
        }

        private static string GetSqlType(Type propertyType)
        {
            if (propertyType == typeof(int))
            {
                return "INT";
            }
            else if (propertyType == typeof(string))
            {
                return "NVARCHAR(MAX)";
            }

            throw new NotSupportedException($"Type {propertyType.Name} is not supported.");
        }
    }
}
