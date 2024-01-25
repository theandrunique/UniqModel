using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SQLModel
{
    internal static class Crud
    {
        public static T GetById<T>(int id, Session session)
        {
            Table table = Metadata.TableClasses[typeof(T)];

            string query = BuildSelectQueryById(table);

            Logging.Info(query);

            return session.Connection.Query<T>(query, new { Id = id }, session.Transaction).FirstOrDefault();
        }
        async public static Task<T> GetByIdAsync<T>(int id, AsyncSession session)
        {
            Table table = Metadata.TableClasses[typeof(T)];

            string query = BuildSelectQueryById(table);

            Logging.Info(query);

            return (await session.Connection.QueryAsync<T>(query, new { Id = id }, session.Transaction)).FirstOrDefault();
        }
        private static string MapFields(Table table)
        {
            return string.Join(", ", table.FieldsRelation.Values.Select(field => $"{field.Name} AS {field.Property.Name}"));
        }
        private static string BuildSelectQueryById(Table table)
        {
            //string idClause = string.Join(" AND ", table.PrimaryKeys.Select(key => $"{key.Name} = {key.Property.GetValue(existedObject)}"));

            return $"SELECT {MapFields(table)} FROM {table.Name} WHERE {table.PrimaryKeys[0].Name} = @Id;";
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
        public static void Create(object newObject, Session session)
        {
            string query = BuildCreateQuery(newObject, session.DbCore.GetLastInsertRowId());

            Logging.Info(query);

            int newObjectId = session.Connection.Query<int>(query, newObject, session.Transaction).FirstOrDefault();

            PrimaryKey key = Metadata.TableClasses[newObject.GetType()].PrimaryKeys[0];

            key.Property.SetValue(newObject, Convert.ChangeType(newObjectId, key.Property.PropertyType));
        }
        async public static Task CreateAsync(object newObject, AsyncSession session)
        {
            string query = BuildCreateQuery(newObject, session.DbCore.GetLastInsertRowId());

            Logging.Info(query);

            int newObjectId = (await session.Connection.QueryAsync<int>(query, newObject, session.Transaction)).FirstOrDefault();

            PrimaryKey key = Metadata.TableClasses[newObject.GetType()].PrimaryKeys[0];

            key.Property.SetValue(newObject, Convert.ChangeType(newObjectId, key.Property.PropertyType));
        }
        public static string GetParams(Dictionary<PropertyInfo, Field> keyValuePairs)
        {
            return string.Join(", ", keyValuePairs.Values.Where(field => !field.PrimaryKey).Select(field => $"@{field.Name}"));
        }
        private static string BuildCreateQuery(object newObject, string lastInsertRowId)
        {
            Type type = newObject.GetType(); // - class

            Table table = Metadata.TableClasses[type];

            string fieldList = GetFields(table.FieldsRelation);

            string paramsList = GetParams(table.FieldsRelation);

            return $"INSERT INTO {table.Name} ({fieldList}) VALUES ({paramsList}); {lastInsertRowId};";
        }
        private static string BuildUpdateQuery(object existedObject)
        {
            Type type = existedObject.GetType();

            Table table = Metadata.TableClasses[type];

            List<PropertyInfo> properties = table.FieldsRelation.Keys.ToList();

            string setClause = string.Join(", ", properties.Select(property =>
            {
                Field field = table.FieldsRelation[property];

                if (field.PrimaryKey)
                {
                    return null;
                }
                var value = property.GetValue(existedObject);

                return $"{field.Name} = @{field.Property.Name}";

            }).Where(fieldValue => fieldValue != null));

            string idClause = string.Join(" AND ", table.PrimaryKeys.Select(key => $"{key.Name} = @{key.Property.Name}"));

            return $"UPDATE {table.Name} SET {setClause} WHERE {idClause};";
        }
        async public static Task UpdateAsync(object existedObject, AsyncSession session)
        {
            string query = BuildUpdateQuery(existedObject);

            Logging.Info(query);

            await session.Connection.ExecuteAsync(query, existedObject, session.Transaction);
        }
        public static void Update(object existedObject, Session session)
        {
            string query = BuildUpdateQuery(existedObject);

            Logging.Info(query);

            session.Connection.Execute(query, existedObject,  session.Transaction);
        }
        private static string BuildDeleteQuery(object existedObject)
        {
            Type type = existedObject.GetType();

            Table table = Metadata.TableClasses[type];

            PrimaryKey primaryKey = table.PrimaryKeys[0];

            string idClause = $"{primaryKey.Name} = @{primaryKey.Property.Name}";

            return $"DELETE FROM {table.Name} WHERE {idClause};";
        }
        public static void Delete(object existedObject, Session session)
        {
            string query = BuildDeleteQuery(existedObject);

            Logging.Info(query);

            session.Connection.Execute(query, existedObject, session.Transaction);
        }
        async public static Task DeleteAsync(object existedObject, AsyncSession session)
        {
            string query = BuildDeleteQuery(existedObject);

            Logging.Info(query);

            await session.Connection.ExecuteAsync(query, existedObject, session.Transaction);
        }
        private static string BuildSelectAllQuery<T>()
        {
            Type type = typeof(T);

            Table table = Metadata.TableClasses[type];

            return $"SELECT {MapFields(table)} FROM {table.Name};";
        }
        public static List<T> GetAll<T>(Session session)
        {
            string query = BuildSelectAllQuery<T>();

            Logging.Info(query);

            List<T> list = session.Connection.Query<T>(query, null, session.Transaction).ToList();

            return list;
        }
        async public static Task<List<T>> GetAllAsync<T>(AsyncSession session)
        {
            string query = BuildSelectAllQuery<T>();

            Logging.Info(query);

            List<T> list = (await session.Connection.QueryAsync<T>(query, null, session.Transaction)).ToList();

            return list;
        }
    }
}
