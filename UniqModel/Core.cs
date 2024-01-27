using Dapper;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace UniqModel
{
    public class Core
    {
        public string ConnectionString { get { return UniqSettings.ConnectionString; } }
        public Metadata Metadata { get { return CoreImpl.Metadata; } }
        public Core(DatabaseEngine databaseEngine, string connectionString, ILogger logger, bool dropErrors)
        {
            CoreImpl.SelectProvider(databaseEngine);

            UniqSettings.ConnectionString = connectionString;
            UniqSettings.DropErrors = dropErrors;
            UniqSettings.AutoCommit = true;

            SetupLogger(logger);

            CoreImpl.Metadata = new Metadata(this);
        }
        public Core(DatabaseEngine databaseEngine, string connectionString)
            : this(databaseEngine, connectionString, null, false) { }
        public Core(DatabaseEngine databaseEngine, string connectionString, bool dropErrors)
            : this(databaseEngine, connectionString, null, dropErrors) { }
        public void SetupLogger(ILogger logger)
        {
            Logging.INIT(logger);
        }
        public Session CreateSession()
        {
            return new Session();
        }
        public async Task<AsyncSession> CreateAsyncSession()
        {
            return await AsyncSession.Create();
        }
        public bool TableExists(string tableName)
        {
            bool temp = UniqSettings.DropErrors;
            UniqSettings.DropErrors = true;
            bool tempLogging = Logging.IsEnabled;
            Logging.IsEnabled = false;

            try
            {
                using (var session = CreateSession())
                {
                    CoreImpl.Execute($"SELECT 1 FROM {tableName} WHERE 1=0", null, session.Connection, session.Transaction);
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                UniqSettings.DropErrors = temp;
                Logging.IsEnabled = tempLogging;
            }
        }
    }
    internal class CoreImpl
    {
        private static IDatabaseProvider _provider { get; set; }
        public static IDatabaseProvider Provider { get { return _provider; } }
        public static Metadata Metadata { get; set; }
        public static void SelectProvider(DatabaseEngine databaseType)
        {
            switch (databaseType)
            {
                case DatabaseEngine.SqlServer:
                    _provider = LoadProvider("UniqModel.SqlServer.dll");
                    break;
                //case DatabaseType.MySql:
                //    databaseProvider = new MySqlDatabaseProvider(); // future
                //    break;
                case DatabaseEngine.Sqlite:
                    _provider = LoadProvider("UniqModel.Sqlite.dll");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), "Unsupported database type");
            }
        }
        private static IDatabaseProvider LoadProvider(string providerName)
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
        public static IEnumerable<T> Query<T>(IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null)
        {
            try
            {
                Logging.Info($"{sql}");
                return conn.Query<T>(sql, param, transaction);
            }
            catch (Exception ex)
            {
                Logging.Error($"{sql} Details: {ex.Message}");
                if (UniqSettings.DropErrors)
                    throw;
                return null;
            }
        }
        public static async Task<IEnumerable<T>> QueryAsync<T>(IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null)
        {
            try
            {
                Logging.Info($"{sql}");
                return await conn.QueryAsync<T>(sql, param, transaction);
            }
            catch (Exception ex)
            {
                Logging.Error($"{sql} Details: {ex.Message}");
                if (UniqSettings.DropErrors)
                    throw;
                return null;
            }
        }
        public static T QueryFirstOrDefault<T>(IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null)
        {
            try
            {
                Logging.Info($"{sql}");
                return conn.QueryFirstOrDefault<T>(sql, param, transaction);
            }
            catch (Exception ex)
            {
                Logging.Error($"{sql} Details: {ex.Message}");
                if (UniqSettings.DropErrors)
                    throw;
                return default(T);
            }
        }
        public static IDataReader ExecuteQuery(string sql, IDbConnection connection, IDbTransaction transaction)
        {
            using (IDbCommand command = _provider.ExecuteCommand(sql, connection, transaction))
            {
                try
                {
                    Logging.Info($"{sql}");

                    return _provider.ExecuteReader(command);
                }
                catch (Exception ex)
                {
                    Logging.Error($"{sql} Details: {ex.Message}");
                    if (UniqSettings.DropErrors)
                        throw;
                    return null;
                }
            }
        }
        async public static Task<IDataReader> ExecuteQueryAsync(string sql, IDbConnection connection, IDbTransaction transaction)
        {
            using (IDbCommand command = await _provider.ExecuteCommandAsync(sql, connection, transaction))
            {
                try
                {
                    Logging.Info($"{sql}");

                    return await _provider.ExecuteReaderAsync(command);
                }
                catch (Exception ex)
                {
                    Logging.Error($"{sql} Details: {ex.Message}");
                    if (UniqSettings.DropErrors)
                        throw;
                    return null;
                }
            }
        }
        public static void Execute(string sql, object param, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                Logging.Info($"{sql}");
                connection.Execute(sql, param, transaction);
            }
            catch (Exception ex)
            {
                Logging.Error($"{sql} Details: {ex.Message}");
                if (UniqSettings.DropErrors)
                    throw;
            }
        }
        async static public Task ExecuteAsync(string sql, object param, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                Logging.Info($"{sql}");
                await connection.ExecuteAsync(sql, param, transaction);
            }
            catch (Exception ex)
            {
                Logging.Error($"{sql} Details: {ex.Message}");
                if (UniqSettings.DropErrors)
                    throw;
            }
        }
        public static async Task<IDbConnection> OpenConnectionAsync()
        {
            return await _provider.OpenConnectionAsync(UniqSettings.ConnectionString);
        }
        public static IDbConnection OpenConnection()
        {
            return _provider.OpenConnection(UniqSettings.ConnectionString);
        }
        public static IDbTransaction BeginTransaction(IDbConnection connection)
        {
            return _provider.BeginTransaction(connection);
        }
        public static async Task<IDbTransaction> BeginTransactionAsync(IDbConnection connection)
        {
            return await _provider.BeginTransactionAsync(connection);
        }
        public static void CommitTransaction(IDbTransaction transaction)
        {
            _provider.CommitTransaction(transaction);
        }
        public static async Task CommitTransactionAsync(IDbTransaction transaction)
        {
            await _provider.CommitTransactionAsync(transaction);
        }
        public static bool ReadReader(IDataReader reader)
        {
            return _provider.Read(reader);
        }
        public static async Task<bool> ReadReaderAsync(IDataReader reader)
        {
            return await _provider.ReadAsync(reader);
        }
        public static async Task CloseReaderAsync(IDataReader reader)
        {
            await _provider.CloseReaderAsync(reader);
        }
        public static void CloseConnection(IDbConnection connection)
        {
            _provider.CloseConnection(connection);
        }
        public static async Task CloseConnectionAsync(IDbConnection connection)
        {
            await _provider.CloseConnectionAsync(connection);
        }
        public static string GetLastInsertRowId()
        {
            return _provider.GetLastInsertRowId();
        }
        public static string GetSqlType(Type type)
        {
            return _provider.GetSqlType(type);
        }
        public static string GetAutoIncrementWithType()
        {
            return _provider.GetAutoIncrementWithType();
        }
    }
    public static class UniqSettings
    {
        public static bool DropErrors { get; set; }
        public static bool AutoCommit { get; set; }
        public static string ConnectionString { get; set; }
    }
}
