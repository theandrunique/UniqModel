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
    public partial class Core
    {
        private string databaseName;
        private string ip;
        public bool IsAuthenticated = false;

        private string username;
        private string password;

        private string connectionString;

        // private static string connectionStringWindows = $"Server=localhost;Database={databaseName};Trusted_Connection=True;";


        public Core(string databaseName, string ip = "localhost")
        {
            this.databaseName = databaseName;
            this.ip = ip;
            // connectionString = $"Server={ip};Database={databaseName};User ID={username};Password={password};";
        }
        public async Task AuthenticateAsync(string password, string username = "sa")
        {
            this.username = username;
            this.password = password;
            string connectionStringTemp = $"Server={ip};Database={databaseName};User ID={username};Password={password};";
            try
            {
                var connection = await OpenConnectionAsync(connectionStringTemp);
                connection.Close();
                IsAuthenticated = true;
                this.connectionString = $"Server={ip};Database={databaseName};User ID={username};Password={password};";
            }
            catch (Exception ex) { throw ex; }
        }

        public void Disconnect()
        {
            IsAuthenticated = false;
            connectionString = string.Empty;
            username = string.Empty;
            password = string.Empty;
        }
        public async Task<SqlConnection> OpenConnectionAsync(string connectionStringTemp = null)
        {
            SqlConnection connection;
            if (!IsAuthenticated)
                connection = new SqlConnection(connectionStringTemp);
            else
                connection = new SqlConnection(this.connectionString);
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
        public SqlConnection OpenConnection(string connectionStringTemp = null)
        {
            SqlConnection connection;
            if (!IsAuthenticated)
                connection = new SqlConnection(connectionStringTemp);
            else
                connection = new SqlConnection(connectionString);

            connection.Open();
            return connection;
        }

        public static SqlDataReader ExecuteQuery(string sql, SqlConnection connection)
        {
            SqlCommand command = new SqlCommand(sql, connection);
            return command.ExecuteReader();
        }

        public static void ExecuteEmptyQuery(string sql, SqlConnection connection)
        {
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        public List<object> GetForeignKeyValues(string referenceTableName, string referenceFieldName)
        {
            List<object> values = new List<object>();

            SqlConnection connection = this.OpenConnection();

            string query = $"SELECT DISTINCT {referenceFieldName} FROM {referenceTableName};";

            SqlDataReader reader = Core.ExecuteQuery(query, connection);
            while (reader.Read())
            {
                values.Add(reader[0]);
            }

            return values;
        }
        public void CreateTables()
        {
            List<Type> typesList = new List<Type>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var types = asm.GetTypes().Where(type => type.BaseType == typeof(BaseModel));
                typesList.AddRange(types);
            }

            // var types = typeof(BaseModel).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(BaseModel)));

            using (var session = new SessionMaker(this))
            {
                // create tables
                foreach (var type in typesList)
                {
                    if (type.IsSubclassOf(typeof(BaseModel)))
                    {
                        TableCreator.CreateTable(type, session);
                    }
                }

                // create foreign keys
                foreach (var type in typesList)
                {
                    if (type.IsSubclassOf(typeof(BaseModel)))
                    {
                        TableCreator.CreateForeignKey(type, session);
                    }
                }
            }
        }
    }
}
