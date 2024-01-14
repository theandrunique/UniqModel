using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SQLModel
{
    internal class Crud
    {
        public static T GetById<T>(int id, Session session)
        {
            string query = BuildSelectQueryById(Metadata.TableClasses[typeof(T)], id);

            using (IDataReader reader = session.Execute(query))
            {
                return MapToObjectAsync<T>(reader, session.DbCore).Result;
            }
        }
        async public static Task<T> GetByIdAsync<T>(int id, AsyncSession session)
        {
            string query = BuildSelectQueryById(Metadata.TableClasses[typeof(T)], id);

            using (IDataReader reader = await session.Execute(query))
            {
                return await MapToObjectAsync<T>(reader, session.DbCore);
            }
        }
        private static string BuildSelectQueryById(Table table, int id)
        {
            return $"SELECT * FROM {table.Name} WHERE id = {id};";
        }
        private static async Task<T> MapToObjectAsync<T>(IDataReader reader, Core core)
        {
            Type type = typeof(T);

            Table table = Metadata.TableClasses[type];

            var properties = table.FieldsRelation.Keys.ToArray();

            T obj = Activator.CreateInstance<T>();

            if (await core.ReadReaderAsync(reader))
            {
                foreach (var item in properties)
                {
                    Field field = table.FieldsRelation[item];

                    item.SetValue(obj, Convert.ChangeType(reader[field.Name], item.PropertyType));
                }
            }

            return obj;
        }
        private static string GetFields(Dictionary<PropertyInfo, Field> keyValuePairs)
        {
            List<PropertyInfo> properties = keyValuePairs.Keys.ToList();

            return string.Join(", ", properties.Select(property =>
            {
                if (keyValuePairs[property].PrimaryKey)
                {
                    return null;
                }
                return keyValuePairs[property].Name;

            }).Where(fieldName => fieldName != null));
        }
        private static string GetValues(Dictionary<PropertyInfo, Field> keyValuePairs, object currentObject, bool withPrimaryKey)
        {
            List<PropertyInfo> properties = keyValuePairs.Keys.ToList();

            return string.Join(", ", properties.Select(property =>
            {
                if (!withPrimaryKey && keyValuePairs[property].PrimaryKey)
                {
                    return null;
                }
                var value = property.GetValue(currentObject);

                return (property.PropertyType == typeof(string) || property.PropertyType == typeof(DateTime)) ? $"'{value}'" : value.ToString();

            }).Where(fieldName => fieldName != null));
        }
        public static void Create(object newObject, Session session)
        {
            string query = BuildCreateQuery(newObject);
            session.ExecuteNonQuery(query);
        }
        async public static Task CreateAsync(object newObject, AsyncSession session)
        {
            string query = BuildCreateQuery(newObject);
            await session.ExecuteNonQuery(query);
        }
        private static string BuildCreateQuery(object newObject)
        {
            Type type = newObject.GetType(); // - class

            Table table = Metadata.TableClasses[type];

            string fieldList = GetFields(table.FieldsRelation);

            string valueList = GetValues(table.FieldsRelation, newObject, false);

            return $"INSERT INTO {table.Name} ({fieldList}) VALUES ({valueList});";
        }
        private static string BuildUpdateQuery(object existedObject)
        {
            Type type = existedObject.GetType();

            Table table = Metadata.TableClasses[type];

            List<PropertyInfo> properties = table.FieldsRelation.Keys.ToList();

            Field primaryKeyField = null;

            string setClause = string.Join(", ", properties.Select(property =>
            {
                Field field = table.FieldsRelation[property];

                if (field.PrimaryKey)
                {
                    primaryKeyField = field;
                    return null;
                }
                var value = property.GetValue(existedObject);

                if (property.PropertyType == typeof(string) || property.PropertyType == typeof(DateTime))
                {
                    return $"{field.Name} = '{value}'";

                } else
                {
                    return $"{field.Name} = {value}";
                }

            }).Where(fieldValue => fieldValue != null));

            string idClause = $"{primaryKeyField.Name} = {primaryKeyField.Property.GetValue(existedObject)}";

            return $"UPDATE {table.Name} SET {setClause} WHERE {idClause};";
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

            Table table = Metadata.TableClasses[type];

            Field primaryKeyField = table.FieldsRelation.Values.ToList().Find(field => field.PrimaryKey);

            string idClause = $"{primaryKeyField.Name} = {primaryKeyField.Property.GetValue(existedObject)}";

            return $"DELETE FROM {table.Name} WHERE {idClause};";
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
        private static string BuildSelectAllQuery<T>()
        {
            Type type = typeof(T);

            Table table = Metadata.TableClasses[type];

            return $"SELECT * FROM {table.Name};";
        }
        private static T CreateInstance<T>(IDataReader reader)
        {
            T obj = Activator.CreateInstance<T>();

            Table table = Metadata.TableClasses[typeof(T)];

            List<PropertyInfo> properties = table.FieldsRelation.Keys.ToList();

            foreach (PropertyInfo item in properties)
            {
                item.SetValue(obj, Convert.ChangeType(reader[table.FieldsRelation[item].Name], item.PropertyType));
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
