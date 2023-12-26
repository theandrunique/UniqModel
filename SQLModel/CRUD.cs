using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SQLModel
{
    public class CRUD
    {
        public static T GetById<T>(int id, SqlConnection conn)
        {
            Type type = typeof(T);
            string query = $"SELECT * FROM {GetTableName(type)} WHERE id = {id};";

            SqlDataReader reader = Core.ExecuteQuery(query, conn);

            if (reader.Read())
            {
                T obj = Activator.CreateInstance<T>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string fieldName = reader.GetName(i);
                    object value = reader.GetValue(i);

                    var fieldInfo = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

                    fieldInfo?.SetValue(obj, value);
                }
                return obj;
            }
            return default;
        }

        public static void Create(object newObject, SqlConnection conn)
        {
            Type type = newObject.GetType();

            FieldInfo[] fields = type.GetFields();

            string fieldList = string.Join(", ", fields.Skip(1).Select(field => field.Name.ToLower()));

            string valueList = string.Join(", ", fields.Skip(1).Select(field =>
            {
                var value = field.GetValue(newObject);
                return field.FieldType == typeof(string) ? $"'{value}'" : value.ToString();
            }));

            string query = $"insert into {GetTableName(type)} ({fieldList}) values ({valueList});";

            Core.ExecuteEmptyQuery(query, conn);
        }
        public static void Update(object existedObject, SqlConnection conn)
        {
            Type type = existedObject.GetType();

            FieldInfo[] fields = type.GetFields();

            //string valueList = string.Join(", ", fields.Skip(1).Select(field =>
            //{
            //    var value = field.GetValue(existedObject);
            //    return field.FieldType == typeof(string) ? $"'{value}'" : value.ToString();
            //}));

            string setClause = string.Join(", ", fields.Skip(1).Select(field =>
            {
                if (field.FieldType == typeof(string) || field.FieldType == typeof(DateTime))
                    return $"{field.Name.ToLower()} = '{field.GetValue(existedObject)}'";

                return $"{field.Name.ToLower()} = {field.GetValue(existedObject)}";
            }));

            string query = $"UPDATE {GetTableName(type)} SET {setClause} WHERE id = {fields[0].GetValue(existedObject)};";

            Core.ExecuteEmptyQuery(query, conn);
        }
        public static void Delete(object existedObject, SqlConnection conn)
        {
            Type type = existedObject.GetType();

            FieldInfo id = type.GetField("Id");

            string query = $"DELETE FROM {GetTableName(type)} WHERE id = {id.GetValue(existedObject)};";

            Core.ExecuteEmptyQuery(query, conn);
        }
        private static string GetTableName(Type type)
        {
            PropertyInfo table = type.GetProperty("TableName");

            return (string)table.GetValue(null);
        }
        public static List<T> GetAll<T>(SqlConnection conn)
        {
            Type type = typeof(T);
            string query = $"SELECT * FROM {GetTableName(type)};";

            SqlDataReader reader = Core.ExecuteQuery(query, conn);

            List<T> list = new List<T>();

            while (reader.Read())
            {
                T obj = Activator.CreateInstance<T>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string fieldName = reader.GetName(i);
                    object value = reader.GetValue(i);

                    var fieldInfo = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

                    fieldInfo?.SetValue(obj, value);
                }

                list.Add(obj);
            }
            return list;
        }
    }
}
