using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
        public IDatabaseProvider DatabaseProvider { get { return databaseProvider; } }
        public Metadata Metadata { get { return metadata; } }
        private Metadata metadata;
        public bool DropErrors { get; set; }
        public Core(DatabaseEngine databaseType, string connectionString, bool loggingInFile = false, string logfileName = "orm.log", bool dropErrors = false)
        {
            metadata = new Metadata(this);

            SelectProvider(databaseType);

            Logging.INIT(loggingInFile, logfileName);

            this.connectionString = connectionString;

            DropErrors = dropErrors;
        }
        private void SelectProvider(DatabaseEngine databaseType)
        {
            switch (databaseType)
            {
                case DatabaseEngine.SqlServer:
                    databaseProvider = new SqlServerDatabaseProvider();
                    break;
                //case DatabaseType.MySql:
                //    databaseProvider = new MySqlDatabaseProvider();
                //    break;
                case DatabaseEngine.Sqlite:
                    databaseProvider = new SqliteDatabaseProvider();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), "Unsupported database type");
            }
        }
        public async Task<IDbConnection> OpenConnectionAsync()
        {
            return await databaseProvider.OpenConnectionAsync(this.connectionString);
        }
        public IDbConnection OpenConnection()
        {
            return databaseProvider.OpenConnection(this.connectionString);
        }
        public IDataReader ExecuteQuery(string sql, IDbConnection connection, IDbTransaction transaction)
        {
            IDbCommand command = databaseProvider.ExecuteCommand(sql, connection, transaction);
            try
            {
                Logging.Info($"{sql}");

                return databaseProvider.ExecuteReader(command);
            }
            catch (Exception ex)
            {
                Logging.Error($"{sql} Details: {ex.Message}");
                throw;
            }
        }
        async public Task<IDataReader> ExecuteQueryAsync(string sql, IDbConnection connection, IDbTransaction transaction)
        {
            IDbCommand command = await databaseProvider.ExecuteCommandAsync(sql, connection, transaction);
            try
            {
                Logging.Info($"{sql}");

                return await databaseProvider.ExecuteReaderAsync(command);
            }
            catch (Exception ex)
            {
                Logging.Error($"{sql} Details: {ex.Message}");
                throw;
            }
        }
        public void ExecuteEmptyQuery(string sql, IDbConnection connection, IDbTransaction transaction)
        {
            using (IDbCommand command = databaseProvider.ExecuteCommand(sql, connection, transaction))
            {
                try
                {
                    Logging.Info($"{sql}");
                    databaseProvider.ExecuteNonQuery(command);
                }
                catch (Exception ex)
                {
                    Logging.Error($"{sql} Details: {ex.Message}");
                    throw;
                }
            }
        }
        async public Task ExecuteEmptyQueryAsync(string sql, IDbConnection connection, IDbTransaction transaction)
        {
            using (IDbCommand command = await databaseProvider.ExecuteCommandAsync(sql, connection, transaction))
            {
                try
                {
                    Logging.Info($"{sql}");
                    await databaseProvider.ExecuteNonQueryAsync(command);
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
            return databaseProvider.BeginTransaction(connection);
        }
        public async Task<IDbTransaction> BeginTransactionAsync(IDbConnection connection)
        {
            return await databaseProvider.BeginTransactionAsync(connection);
        }
        public void CommitTransaction(IDbTransaction transaction)
        {
            databaseProvider.CommitTransaction(transaction);
        }
        public async Task CommitTransactionAsync(IDbTransaction transaction)
        {
            await databaseProvider.CommitTransactionAsync(transaction);
        }
        public bool ReadReader(IDataReader reader)
        {
            return databaseProvider.Read(reader);
        }
        public async Task<bool> ReadReaderAsync(IDataReader reader)
        {
            return await databaseProvider.ReadAsync(reader);
        }
        public void CloseConnection(IDbConnection connection)
        {
            databaseProvider.CloseConnection(connection);
        }
        public async Task CloseConnectionAsync(IDbConnection connection)
        {
            await databaseProvider.CloseConnectionAsync(connection);
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
        public string GetAutoIncrementWithType()
        {
            return databaseProvider.GetAutoIncrementWithType();
        }
        public Session CreateSession()
        {
            return new Session(this);
        }
        public async Task<AsyncSession> CreateAsyncSession()
        {
            return await AsyncSession.Create(this);
        }
        public bool TableExists(string tableName)
        {
            bool temp = DropErrors;
            DropErrors = true;
            Logging.IsEnabled = false;
            try
            {
                using (var session = this.CreateSession())
                {
                    ExecuteEmptyQuery($"SELECT 1 FROM {tableName} WHERE 1=0", session.Connection, session.Transaction);
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                DropErrors = temp;
                Logging.IsEnabled = true;
            }
        }
    }
}
