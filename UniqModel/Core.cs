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
        public Core(DatabaseEngine databaseEngine, string connectionString, ILogger logger, bool debagLog, bool dropErrors)
        {
            CoreImpl.SelectProvider(databaseEngine);

            UniqSettings.ConnectionString = connectionString;
            UniqSettings.DropErrors = dropErrors;
            UniqSettings.AutoCommit = true;

            if (logger != null)
            {
                Logging.INIT(logger);
                UniqSettings.Logging = true;
                if (debagLog == true)
                {
                    UniqSettings.ShowValuesInLog = true;
                }
            }
            SelectExecutor();

            CoreImpl.Metadata = new Metadata(this);
        }
        public Core(DatabaseEngine databaseEngine, string connectionString, ILogger logger, bool dropErrors) 
            : this(databaseEngine, connectionString, logger, false, dropErrors) { }
        public Core(DatabaseEngine databaseEngine, string connectionString)
            : this(databaseEngine, connectionString, null, false) { }
        public Core(DatabaseEngine databaseEngine, string connectionString, bool dropErrors)
            : this(databaseEngine, connectionString, null, dropErrors) { }
        private void SelectExecutor()
        {
            if (UniqSettings.Logging)
            {
                if (UniqSettings.ShowValuesInLog)
                {
                    CoreImpl.QueryExecutor = new QueryExecutorWithDebugLogs();
                }
                else
                {
                    CoreImpl.QueryExecutor = new QueryExecutorWithLogs();
                }
            }
            else
            {
                CoreImpl.QueryExecutor = new QueryExecutor();
            }
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
        public static IQueryExecutor QueryExecutor { get; set; }
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
            return QueryExecutor.Query<T>(sql, param, conn, transaction);
        }
        public static async Task<IEnumerable<T>> QueryAsync<T>(IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null)
        {
            return await QueryExecutor.QueryAsync<T>(sql, param, conn, transaction);
        }
        public static T QueryFirstOrDefault<T>(IDbConnection conn, string sql, object param = null, IDbTransaction transaction = null)
        {
            return QueryExecutor.QueryFirstOrDefault<T>(sql, param, conn, transaction);
        }
        public static IDataReader ExecuteQuery(string sql, IDbConnection conn, IDbTransaction transaction)
        {
            using (IDbCommand command = _provider.ExecuteCommand(sql, conn, transaction))
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
        async public static Task<IDataReader> ExecuteQueryAsync(string sql, IDbConnection conn, IDbTransaction transaction)
        {
            using (IDbCommand command = await _provider.ExecuteCommandAsync(sql, conn, transaction))
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
        public static void Execute(string sql, object param, IDbConnection conn, IDbTransaction transaction)
        {
            QueryExecutor.Execute(sql, param, conn, transaction);
        }
        async static public Task ExecuteAsync(string sql, object param, IDbConnection connection, IDbTransaction transaction)
        {
            await QueryExecutor.ExecuteAsync(sql, param, connection, transaction);
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
    internal interface IQueryExecutor
    {
        IEnumerable<T> Query<T>(string sql, object param, IDbConnection conn,  IDbTransaction transaction);
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object param, IDbConnection conn, IDbTransaction transaction);
        T QueryFirstOrDefault<T>(string sql, object param, IDbConnection conn, IDbTransaction transaction);
        void Execute(string sql, object param, IDbConnection connection, IDbTransaction transaction);
        Task ExecuteAsync(string sql, object param, IDbConnection connection, IDbTransaction transaction);
    }
    internal class QueryExecutor : IQueryExecutor
    {
        public IEnumerable<T> Query<T>(string sql, object param, IDbConnection conn, IDbTransaction transaction)
        {
            try
            {
                return conn.Query<T>(sql, param, transaction);
            }
            catch
            {
                if (UniqSettings.DropErrors)
                    throw;
                return null;
            }
        }
        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param, IDbConnection conn, IDbTransaction transaction)
        {
            try
            {
                return await conn.QueryAsync<T>(sql, param, transaction);
            }
            catch
            {
                if (UniqSettings.DropErrors)
                    throw;
                return null;
            }
        }
        public T QueryFirstOrDefault<T>(string sql, object param, IDbConnection conn, IDbTransaction transaction)
        {
            try
            {
                return conn.QueryFirstOrDefault<T>(sql, param, transaction);
            }
            catch
            {
                if (UniqSettings.DropErrors)
                    throw;
                return default(T);
            }
        }
        public void Execute(string sql, object param, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                connection.Execute(sql, param, transaction);
            }
            catch
            {
                if (UniqSettings.DropErrors)
                    throw;
            }
        }
        async public Task ExecuteAsync(string sql, object param, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                await connection.ExecuteAsync(sql, param, transaction);
            }
            catch (Exception ex)
            {
                if (UniqSettings.DropErrors)
                    throw;
            }
        }
    }
    internal class QueryExecutorWithLogs : IQueryExecutor
    {
        public IEnumerable<T> Query<T>(string sql, object param, IDbConnection conn, IDbTransaction transaction)
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
        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param, IDbConnection conn, IDbTransaction transaction)
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
        public T QueryFirstOrDefault<T>(string sql, object param, IDbConnection conn, IDbTransaction transaction)
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
        public void Execute(string sql, object param, IDbConnection connection, IDbTransaction transaction)
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
        async public Task ExecuteAsync(string sql, object param, IDbConnection connection, IDbTransaction transaction)
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
    }
    internal class QueryExecutorWithDebugLogs : IQueryExecutor
    {
        private string GetParamString(object param)
        {
            if (param == null)
                return string.Empty;

            var properties = param.GetType().GetProperties();

            var keyValuePairs = properties.Select(p => $"{p.Name}={p.GetValue(param)}");

            return string.Join(", ", keyValuePairs);
        }
        public IEnumerable<T> Query<T>(string sql, object param, IDbConnection conn,  IDbTransaction transaction)
        {
            try
            {
                Logging.Info($"{sql}");
                string paramStr = GetParamString(param);
                if (!string.IsNullOrEmpty(paramStr))
                {
                    Logging.Debug($"({paramStr})");
                }

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
        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param, IDbConnection conn, IDbTransaction transaction)
        {
            try
            {
                Logging.Info($"{sql}");
                string paramStr = GetParamString(param);
                if (!string.IsNullOrEmpty(paramStr))
                {
                    Logging.Debug($"({paramStr})");
                }

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
        public T QueryFirstOrDefault<T>(string sql, object param, IDbConnection conn, IDbTransaction transaction)
        {
            try
            {
                Logging.Info($"{sql}");
                string paramStr = GetParamString(param);
                if (!string.IsNullOrEmpty(paramStr))
                {
                    Logging.Debug($"({paramStr})");
                }

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
        public void Execute(string sql, object param, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                Logging.Info($"{sql}");
                string paramStr = GetParamString(param);
                if (!string.IsNullOrEmpty(paramStr))
                {
                    Logging.Debug($"({paramStr})");
                }

                connection.Execute(sql, param, transaction);
            }
            catch (Exception ex)
            {
                Logging.Error($"{sql} Details: {ex.Message}");
                if (UniqSettings.DropErrors)
                    throw;
            }
        }
        async public Task ExecuteAsync(string sql, object param, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                Logging.Info($"{sql}");
                string paramStr = GetParamString(param);
                if (!string.IsNullOrEmpty(paramStr))
                {
                    Logging.Debug($"({paramStr})");
                }

                await connection.ExecuteAsync(sql, param, transaction);
            }
            catch (Exception ex)
            {
                Logging.Error($"{sql} Details: {ex.Message}");
                if (UniqSettings.DropErrors)
                    throw;
            }
        }
    }

    public static class UniqSettings
    {
        public static bool DropErrors { get; set; } = true;
        public static bool AutoCommit { get; set; } = true;
        public static string ConnectionString { get; set; }
        public static bool ShowValuesInLog { get; set; } = false;
        public static bool Logging { get; set; } = false;
    }
}
