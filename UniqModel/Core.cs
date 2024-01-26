using NLog;
using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace UniqModel
{
    public class Core
    {
        public string ConnectionString { get { return connectionString; } }
        private string connectionString;
        private static IDatabaseProvider databaseProvider;
        public IDatabaseProvider DatabaseProvider { get { return databaseProvider; } }
        public Metadata Metadata { get { return metadata; } }
        private Metadata metadata;
        public bool DropErrors { get; set; }
        public bool AutoCommit { get; set; } = true;
        public Core(DatabaseEngine databaseEngine, string connectionString, ILogger logger, bool dropErrors)
        {
            SelectProvider(databaseEngine);

            this.connectionString = connectionString;

            SetupLogger(logger);

            DropErrors = dropErrors;

            metadata = new Metadata(this);
        }
        public Core(DatabaseEngine databaseEngine, string connectionString)
            : this(databaseEngine, connectionString, null, false) { }
        public Core(DatabaseEngine databaseEngine, string connectionString, bool dropErrors)
            : this(databaseEngine, connectionString, null, dropErrors) { }
        public void SetupLogger(ILogger logger)
        {
            Logging.INIT(logger);
        }
        private void SelectProvider(DatabaseEngine databaseType)
        {
            switch (databaseType)
            {
                case DatabaseEngine.SqlServer:
                    databaseProvider = LoadProvider("UniqModel.SqlServer.dll");
                    break;
                //case DatabaseType.MySql:
                //    databaseProvider = new MySqlDatabaseProvider(); // future
                //    break;
                case DatabaseEngine.Sqlite:
                    databaseProvider = LoadProvider("UniqModel.Sqlite.dll");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), "Unsupported database type");
            }
        }
        private IDatabaseProvider LoadProvider(string providerName)
        {
            Assembly assembly = Assembly.LoadFrom(providerName);

            Type providerType = assembly.GetTypes()
                .FirstOrDefault(t => typeof(IDatabaseProvider).IsAssignableFrom(t));

            if (providerType != null)
            {
                return (IDatabaseProvider)Activator.CreateInstance(providerType);
            }

            throw new ArgumentException($"Provider '{providerName}' not supported.");
        }
        public async Task<IDbConnection> OpenConnectionAsync()
        {
            return await databaseProvider.OpenConnectionAsync(connectionString);
        }
        public IDbConnection OpenConnection()
        {
            return databaseProvider.OpenConnection(connectionString);
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
        public async Task CloseReaderAsync(IDataReader reader)
        {
            await databaseProvider.CloseReaderAsync(reader);
        }
        public void CloseConnection(IDbConnection connection)
        {
            databaseProvider.CloseConnection(connection);
        }
        public async Task CloseConnectionAsync(IDbConnection connection)
        {
            await databaseProvider.CloseConnectionAsync(connection);
        }
        public string GetLastInsertRowId()
        {
            return databaseProvider.GetLastInsertRowId();
        }
        public static string GetSqlType(Type type)
        {
            return databaseProvider.GetSqlType(type);
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
            bool tempLogging = Logging.IsEnabled;
            Logging.IsEnabled = false;

            try
            {
                using (var session = CreateSession())
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
                Logging.IsEnabled = tempLogging;
            }
        }
    }
    internal class CoreImpl
    {
        public static IDatabaseProvider Provider { get; set; }
        public string ConnectionString { get { return connectionString; } }
        private string connectionString;
    }
}
