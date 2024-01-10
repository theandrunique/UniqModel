using NLog.Targets;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;

namespace SQLModel
{
    public class Core
    {
        private string connectionString;

        public Core(string connectionString, bool createTables = false, bool loggingInFile = false, string logfileName = "orm.log")
        {
            this.connectionString = connectionString;

            Logging.INIT(loggingInFile, logfileName);

            if (createTables)
            {
                CreateTables();
            }

            CheckExistedTables();
        }
        public async Task<SqlConnection> OpenConnectionAsync()
        {
            SqlConnection connection = new SqlConnection(this.connectionString);
            try
            {
                await connection.OpenAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return connection;
        }
        public SqlConnection OpenConnection()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public SqlDataReader ExecuteQuery(string sql, SqlConnection connection, SqlTransaction transaction)
        {
            SqlCommand command = new SqlCommand(sql, connection, transaction);
            try
            {
                Logging.Info($"{sql}");

                return command.ExecuteReader();
            }
            catch (Exception ex)
            {
                Logging.Error($"{sql} Details: {ex.Message}");
                throw;
            }
        }
        async public Task<SqlDataReader> ExecuteQueryAsync(string sql, SqlConnection connection, SqlTransaction transaction)
        {
            SqlCommand command = new SqlCommand(sql, connection, transaction);
            try
            {
                Logging.Info($"{sql}");

                return await command.ExecuteReaderAsync();
            }
            catch (Exception ex)
            {
                Logging.Error($"{sql} Details: {ex.Message}");
                throw;
            }
        }
        public void ExecuteEmptyQuery(string sql, SqlConnection connection, SqlTransaction transaction)
        {
            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                try
                {
                    Logging.Info($"{sql}");
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Logging.Error($"{sql} Details: {ex.Message}");
                    throw;
                }
            }
        }
        async public Task ExecuteEmptyQueryAsync(string sql, SqlConnection connection, SqlTransaction transaction)
        {
            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                try
                {
                    Logging.Info($"{sql}");
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    Logging.Error($"{sql} Details: {ex.Message}");
                    throw;
                }
            }
        }
        public List<object> GetForeignKeyValues(string referenceTableName, Session session, string referenceFieldName = "id")
        {
            List<object> values = new List<object>();

            string query = $"SELECT DISTINCT {referenceFieldName} FROM {referenceTableName};";

            using (SqlDataReader reader = session.Execute(query))
            {
                while (reader.Read())
                {
                    values.Add(reader[referenceFieldName]);
                }
                return values;
            }
        }
        public Session CreateSession()
        {
            return new Session(this);
        }
        public async Task<AsyncSession> CreateAsyncSession()
        {
            return await AsyncSession.Create(this);
        }
        private void CreateTables()
        {
            List<Type> typesList = GetBaseModelTypes();

            foreach (var type in typesList)
            {
                using (var session = new Session(this))
                {
                    TableCreator.CreateTable(type, session);
                }
            }
            // create foreign keys
            foreach (var type in typesList)
            {
                using (var session = new Session(this))
                {
                    TableCreator.CreateForeignKey(type, session);
                }
            }
        }
        private void CheckExistedTables()
        {
            List<Type> typesList = GetBaseModelTypes();

            foreach (var type in typesList)
            {
                var tableAttribute = (TableAttribute)type.GetCustomAttribute(typeof(TableAttribute));
                if (tableAttribute == null)
                {
                    throw new ArgumentException("The class must be marked with TableAttribute.");
                }
                if (!TableExists(tableAttribute.TableName, this))
                    throw new Exception($"Table {tableAttribute.TableName} does not exists");
            }
        }
        static List<Type> GetBaseModelTypes()
        {
            List<Type> typesList = new List<Type>();

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var types = asm.GetTypes().Where(type => type.BaseType == typeof(BaseModel));
                typesList.AddRange(types);
            }

            return typesList;
        }
        static bool TableExists(string tableName, Core dbcore)
        {
            using (SqlConnection connection = dbcore.OpenConnection())
            {
                using (SqlCommand command = new SqlCommand($"SELECT 1 FROM {tableName} WHERE 1=0", connection))
                {
                    try
                    {
                        command.ExecuteNonQuery();
                        Logging.Info($"Table {tableName} is verified");
                        return true;
                    }
                    catch (SqlException ex)
                    {
                        Logging.Critical($"Table {tableName} does not exist. Please check the database schema. Detail: {ex.Message}");
                        return false;
                    }
                }
            }
        }
    }
}
