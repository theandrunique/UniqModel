using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SQLModel
{
    public class Core
    {
        private string connectionString;
        private IDatabaseProvider databaseProvider;

        public Core(DatabaseType databaseType, string connectionString, bool createTables = false, bool loggingInFile = false, string logfileName = "orm.log")
        {
            SelectProvider(databaseType);

            this.connectionString = connectionString;

            Logging.INIT(loggingInFile, logfileName);

            if (createTables)
            {
                CreateTables();
            }
            //CheckExistedTables();
        }
        private void SelectProvider(DatabaseType databaseType)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    databaseProvider = new SqlServerDatabaseProvider();
                    break;
                //case DatabaseType.MySql:
                //    databaseProvider = new MySqlDatabaseProvider();
                //    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), "Unsupported database type");
            }
        }
        public async Task<IDbConnection> OpenConnectionAsync()
        {
            return await OpenConnectionIternal();
        }
        public IDbConnection OpenConnection()
        {
            return OpenConnectionIternal().GetAwaiter().GetResult();
        }
        async private Task<IDbConnection> OpenConnectionIternal()
        {
            return await databaseProvider.OpenConnectionIternal(this.connectionString);
        }
        public IDataReader ExecuteQuery(string sql, IDbConnection connection, IDbTransaction transaction)
        {
            return ExecuteQueryIternal(sql, connection, transaction).GetAwaiter().GetResult();
        }
        async public Task<IDataReader> ExecuteQueryAsync(string sql, IDbConnection connection, IDbTransaction transaction)
        {
            return await ExecuteQueryIternal(sql, connection, transaction);
        }
        async private Task<IDataReader> ExecuteQueryIternal(string sql, IDbConnection connection, IDbTransaction transaction)
        {
            IDbCommand command = await databaseProvider.ExecuteCommand(sql, connection, transaction);
            try
            {
                Logging.Info($"{sql}");

                return await databaseProvider.ExecuteReader(command);
            }
            catch (Exception ex)
            {
                Logging.Error($"{sql} Details: {ex.Message}");
                throw;
            }
        }
        public void ExecuteEmptyQuery(string sql, IDbConnection connection, IDbTransaction transaction)
        {
            ExecuteEmptyQueryIternal(sql, connection, transaction).Wait();
        }
        async public Task ExecuteEmptyQueryAsync(string sql, IDbConnection connection, IDbTransaction transaction)
        {
            await ExecuteEmptyQueryIternal(sql, connection, transaction);
        }
        async private Task ExecuteEmptyQueryIternal(string sql, IDbConnection connection, IDbTransaction transaction)
        {
            using (IDbCommand command = await databaseProvider.ExecuteCommand(sql, connection, transaction))
            {
                try
                {
                    Logging.Info($"{sql}");
                    await databaseProvider.ExecuteNonQuery(command);
                }
                catch (Exception ex)
                {
                    Logging.Error($"{sql} Details: {ex.Message}");
                    throw;
                }
            }
        }
        public IDbTransaction BeginTransaction(IDbConnection connection)
        {
            return BeginTransactionIternal(connection).GetAwaiter().GetResult();
        }
        public async Task<IDbTransaction> BeginTransactionAsync(IDbConnection connection)
        {
            return await BeginTransactionIternal(connection);
        }
        async private Task<IDbTransaction> BeginTransactionIternal(IDbConnection connection)
        {
            return await databaseProvider.BeginTransaction(connection);
        }
        public void CommitTransaction(IDbTransaction transaction)
        {
            CommitTransactionIternal(transaction).GetAwaiter().GetResult();
        }
        public async Task CommitTransactionAsync(IDbTransaction transaction)
        {
            await CommitTransactionIternal(transaction);
        }
        async private Task CommitTransactionIternal(IDbTransaction transaction)
        {
            await databaseProvider.CommitTransaction(transaction);
        }
        public bool ReadReader(IDataReader reader)
        {
            return ReadReaderIternal(reader).GetAwaiter().GetResult();
        }
        public async Task<bool> ReadReaderAsync(IDataReader reader)
        {
             return await ReadReaderIternal(reader);
        }
        private async Task<bool> ReadReaderIternal(IDataReader reader)
        {
            return await databaseProvider.Read(reader);
        }
        public void CloseConnection(IDbConnection connection)
        {
            CloseConnectionIternal(connection).GetAwaiter().GetResult();
        }
        public async Task CloseConnectionAsync(IDbConnection connection)
        {
            await CloseConnectionIternal(connection);
        }
        async private Task CloseConnectionIternal(IDbConnection connection)
        {
            await databaseProvider.CloseConnection(connection);
        }
        public List<object> GetForeignKeyValues(string referenceTableName, Session session, string referenceFieldName = "id")
        {
            List<object> values = new List<object>();

            string query = $"SELECT DISTINCT {referenceFieldName} FROM {referenceTableName};";

            using (IDataReader reader = session.Execute(query))
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
        //private void CheckExistedTables()
        //{
        //    List<Type> typesList = GetBaseModelTypes();

        //    foreach (var type in typesList)
        //    {
        //        var tableAttribute = (TableAttribute)type.GetCustomAttribute(typeof(TableAttribute));
        //        if (tableAttribute == null)
        //        {
        //            throw new ArgumentException("The class must be marked with TableAttribute.");
        //        }
        //        if (!TableExists(tableAttribute.TableName, this))
        //            throw new Exception($"Table {tableAttribute.TableName} does not exists");
        //    }
        //}
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
        //static bool TableExists(string tableName, Core dbcore)
        //{
        //    using (IDbConnection connection = dbcore.OpenConnection())
        //    {
        //        using (IDbCommand command = new SqlCommand($"SELECT 1 FROM {tableName} WHERE 1=0", connection))
        //        {
        //            try
        //            {
        //                command.ExecuteNonQuery();
        //                Logging.Info($"Table {tableName} is verified");
        //                return true;
        //            }
        //            catch (SqlException ex)
        //            {
        //                Logging.Critical($"Table {tableName} does not exist. Please check the database schema. Detail: {ex.Message}");
        //                return false;
        //            }
        //        }
        //    }
        //}
    }
}
