using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SQLModel
{
    internal class AsyncCRUD
    {
        async public static Task<T> GetById<T>(int id, AsyncSession session)
        {
            Type type = typeof(T);
            string query = $"SELECT * FROM {GetTableName(type)} WHERE id = {id};";

            using (SqlDataReader reader = await session.Execute(query))
            {
                if (await reader.ReadAsync())
                {
                    var properties = type.GetProperties();
                    T obj = Activator.CreateInstance<T>();
                    foreach (var item in properties)
                    {
                        FieldAttribute fieldAttribute = item.GetCustomAttribute<FieldAttribute>();

                        if (fieldAttribute != null)
                        {
                            item.SetValue(obj, reader[fieldAttribute.ColumnName]);
                        }
                    }
                    return obj;
                }
            }
            return default;
        }

        async public static Task Create(object newObject, AsyncSession session)
        {
            Type type = newObject.GetType();

            var fields = type.GetProperties();

            string fieldList = string.Join(", ", fields.Skip(1).Select(field =>
            {
                FieldAttribute fieldAttribute = field.GetCustomAttribute<FieldAttribute>();

                if (fieldAttribute != null)
                {
                    return fieldAttribute.ColumnName;
                }

                return null;
            }).Where(fieldName => fieldName != null));

            string valueList = string.Join(", ", fields.Skip(1).Select(field =>
            {
                FieldAttribute fieldAttribute = field.GetCustomAttribute<FieldAttribute>();

                if (fieldAttribute != null)
                {
                    var value = field.GetValue(newObject);

                    return (field.PropertyType == typeof(string) || field.PropertyType == typeof(DateTime)) ? $"'{value}'" : value.ToString();
                }

                return null;
            }).Where(fieldValue => fieldValue != null));

            string query = $"insert into {GetTableName(type)} ({fieldList}) values ({valueList});";

            await session.ExecuteNonQuery(query);
        }
        async public static Task Update(object existedObject, AsyncSession session)
        {
            Type type = existedObject.GetType();

            var fields = type.GetProperties();

            string setClause = string.Join(", ", fields.Skip(1).Select(field =>
            {
                FieldAttribute fieldAttribute = field.GetCustomAttribute<FieldAttribute>();

                if (fieldAttribute != null)
                {
                    var value = field.GetValue(existedObject);

                    if (field.PropertyType == typeof(string) || field.PropertyType == typeof(DateTime))
                        return $"{fieldAttribute.ColumnName} = '{field.GetValue(existedObject)}'";

                    return $"{fieldAttribute.ColumnName} = {field.GetValue(existedObject)}";
                }
                return null;
            }).Where(fieldValue => fieldValue != null));

            string query = $"UPDATE {GetTableName(type)} SET {setClause} WHERE id = {fields[0].GetValue(existedObject)};";

            await session.ExecuteNonQuery(query);
        }
        async public static Task Delete(object existedObject, AsyncSession session)
        {
            Type type = existedObject.GetType();

            var id = type.GetProperty("Id");

            string query = $"DELETE FROM {GetTableName(type)} WHERE id = {id.GetValue(existedObject)};";

            await session.ExecuteNonQuery(query);
        }
        private static string GetTableName(Type type)
        {
            var tableAttribute = (TableAttribute)type.GetCustomAttribute(typeof(TableAttribute));

            return tableAttribute.TableName;
        }
        async public static Task<List<T>> GetAll<T>(AsyncSession session)
        {
            Type type = typeof(T);
            string query = $"SELECT * FROM {GetTableName(type)};";
            List<T> list = new List<T>();

            using (SqlDataReader reader = await session.Execute(query))
            {
                while ( await reader.ReadAsync() )
                {
                    T obj = Activator.CreateInstance<T>();
                    var properties = type.GetProperties();

                    foreach (var item in properties)
                    {
                        FieldAttribute fieldAttribute = item.GetCustomAttribute<FieldAttribute>();

                        if (fieldAttribute != null)
                        {
                            item.SetValue(obj, reader[fieldAttribute.ColumnName]);
                        }
                    }
                    list.Add(obj);
                }
            }
            return list;

        }
    }
}
