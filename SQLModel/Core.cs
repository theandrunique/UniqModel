using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SQLModel
{
    public class Core
    {
        // public bool IsAuthenticated = false;

        private string connectionString;
        
        // private static string connectionStringWindows = $"Server=localhost;Database={databaseName};Trusted_Connection=True;";

        public Core(string connectionString, bool createTables = false, bool loggingInFile = false, bool consoleLog = false, string logfileName = "orm.log")
        {
            this.connectionString = connectionString;
            if (loggingInFile)
            {
                Logger.IsEnabled = loggingInFile;
                Logger.LogfileName = logfileName;
            }
            if (consoleLog)
            {
                Logger.IsDebugConsoleOutputEnabled = consoleLog;
                Logger.InitConsoleDebagLogger();
            }
            if (createTables)
            {
                CreateTables();
            }

            CheckExistedTables();
        }
        //public void AuthenticateSync(string password, string username = "sa")
        //{
        //    this.username = username;
        //    this.password = password;
        //    string connectionStringTemp = $"Server={ip};Database={databaseName};User ID={username};Password={password};Trusted_Connection=True;";
        //    try
        //    {
        //        var connection = OpenConnection(connectionStringTemp);
        //        connection.Close();
        //        IsAuthenticated = true;
        //        this.connectionString = $"Server={ip};Database={databaseName};User ID={username};Password={password};Trusted_Connection=True;";
        //    }
        //    catch (Exception ex) { throw ex; }
        //}
        //public async Task AuthenticateAsync(string password, string username = "sa")
        //{
        //    this.username = username;
        //    this.password = password;
        //    string connectionStringTemp = $"Server={ip};Database={databaseName};User ID={username};Password={password};Trusted_Connection=True;";
        //    try
        //    {
        //        var connection = await OpenConnectionAsync(connectionStringTemp);
        //        connection.Close();
        //        IsAuthenticated = true;
        //        this.connectionString = $"Server={ip};Database={databaseName};User ID={username};Password={password};Trusted_Connection=True;";
        //    }
        //    catch (Exception ex) { throw ex; }
        //}

        //public void Disconnect()
        //{
        //    IsAuthenticated = false;
        //    connectionString = string.Empty;
        //    username = string.Empty;
        //    password = string.Empty;
        //}
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
                Logger.Info($"{sql}");

                return command.ExecuteReader();
            }
            catch (Exception ex)
            {
                Logger.Error($"{sql} Details: {ex.Message}");
                throw;
            }
        }

        public void ExecuteEmptyQuery(string sql, SqlConnection connection, SqlTransaction transaction)
        {
            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                try
                {
                    Logger.Info($"{sql}");
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Logger.Error($"{sql} Details: {ex.Message}");
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
        private void CreateTables()
        {
            List<Type> typesList = GetBaseModelTypes();

            // var types = typeof(BaseModel).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(BaseModel)));

            // create tables
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
                        Logger.Info($"Table {tableName} is verified");
                        return true;
                    }
                    catch (SqlException ex)
                    {
                        Logger.Critical($"Table {tableName} does not exist. Please check the database schema. Detail: {ex.Message}");
                        return false;
                    }
                }
            }
        }
    }
}
