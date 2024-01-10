using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SQLModel
{
    internal class Crud
    {
        public static T GetById<T>(int id, Session session)
        {
            string query = BuildSelectQueryById<T>(id);

            using (IDataReader reader = session.Execute(query))
            {
                return MapToObjectAsync<T>(reader, session.DbCore).GetAwaiter().GetResult();
            }
        }
        async public static Task<T> GetByIdAsync<T>(int id, AsyncSession session)
        {
            string query = BuildSelectQueryById<T>(id);

            using (IDataReader reader = await session.Execute(query))
            {
                return await MapToObjectAsync<T>(reader, session.DbCore);
            }
        }
        private static string BuildSelectQueryById<T>(int id)
        {
            Type type = typeof(T);
            return $"SELECT * FROM {GetTableName(type)} WHERE id = {id};";
        }
        private static async Task<T> MapToObjectAsync<T>(IDataReader reader, Core core)
        {
            Type type = typeof(T);
            var properties = type.GetProperties();
            T obj = Activator.CreateInstance<T>();

            if (await core.ReadReaderAsync(reader))
            {
                foreach (var item in properties)
                {
                    FieldAttribute fieldAttribute = item.GetCustomAttribute<FieldAttribute>();

                    if (fieldAttribute != null)
                    {
                        item.SetValue(obj, reader[fieldAttribute.ColumnName]);
                    }
                }
            }

            return obj;
        }
        private static string BuildCreateQuery(object newObject)
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
            return $"insert into {GetTableName(type)} ({fieldList}) values ({valueList});";
        }
        public static void Create(object newObject, Session session)
        {
            Type type = newObject.GetType();
            string query = BuildCreateQuery(newObject);
            session.ExecuteNonQuery(query);
        }
        async public static Task CreateAsync(object newObject, AsyncSession session)
        {
            Type type = newObject.GetType();
            string query = BuildCreateQuery(type);
            await session.ExecuteNonQuery(query);
        }
        private static string BuildUpdateQuery(object existedObject)
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

            return $"UPDATE {GetTableName(type)} SET {setClause} WHERE id = {fields[0].GetValue(existedObject)};";
        }
        async public static Task UpdateAsync(object existedObject, AsyncSession session)
        {
            string query = BuildUpdateQuery(existedObject);
            await session.ExecuteNonQuery(query);
        }
        public static void Update(object existedObject, Session session)
        {
            string query = BuildUpdateQuery(existedObject);
            session.ExecuteNonQuery(query);
        }
        private static string BuildDeleteQuery(object existedObject)
        {
            Type type = existedObject.GetType();

            var id = type.GetProperty("Id");

            return $"DELETE FROM {GetTableName(type)} WHERE id = {id.GetValue(existedObject)};";
        }
        public static void Delete(object existedObject, Session session)
        {
            string query = BuildDeleteQuery(existedObject);

            session.ExecuteNonQuery(query);
        }
        async public static Task DeleteAsync(object existedObject, AsyncSession session)
        {
            string query = BuildDeleteQuery(existedObject);

            await session.ExecuteNonQuery(query);
        }
        private static string GetTableName(Type type)
        {
            var tableAttribute = (TableAttribute)type.GetCustomAttribute(typeof(TableAttribute));

            return tableAttribute.TableName;
        }
        private static string BuildSelectAllQuery<T>()
        {
            Type type = typeof(T);
            return $"SELECT * FROM {GetTableName(type)};";
        }
        private static T CreateInstance<T>(IDataReader reader)
        {
            T obj = Activator.CreateInstance<T>();
            var properties = typeof(T).GetProperties();

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
        public static List<T> GetAll<T>(Session session)
        {
            string query = BuildSelectAllQuery<T>();
            List<T> list = new List<T>();

            using (IDataReader reader = session.Execute(query))
            {
                while (reader.Read())
                {
                    T obj = CreateInstance<T>(reader);
                    list.Add(obj);
                }
            }
            return list;
        }
        async public static Task<List<T>> GetAllAsync<T>(AsyncSession session)
        {
            string query = BuildSelectAllQuery<T>();
            List<T> list = new List<T>();

            using (IDataReader reader = await session.Execute(query))
            {
                while (await session.ReadAsync(reader))
                {
                    T obj = CreateInstance<T>(reader);
                    list.Add(obj);
                }
            }
            return list;
        }
    }
}
