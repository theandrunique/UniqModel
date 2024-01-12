using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SQLModel
{
    internal class SqlServerDatabaseProvider : IDatabaseProvider
    {
        async public Task<IDbConnection> OpenConnectionAsync(string connectionString)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            return conn;
        }
        public IDbConnection OpenConnection(string connectionString)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            return conn;
        }
        async public Task<IDbTransaction> BeginTransactionAsync(IDbConnection connection)
        {
            return ((SqlConnection)connection).BeginTransaction();
        }
        public IDbTransaction BeginTransaction(IDbConnection connection)
        {
            return ((SqlConnection)connection).BeginTransaction();
        }
        async public Task CloseConnectionAsync(IDbConnection connection)
        {
            ((SqlConnection)connection).Close();
        }
        public void CloseConnection(IDbConnection connection)
        {
            ((SqlConnection)connection).Close();
        }
        async public Task CommitTransactionAsync(IDbTransaction transaction)
        {
            ((SqlTransaction)transaction).Commit();
        }
        public void CommitTransaction(IDbTransaction transaction)
        {
            ((SqlTransaction)transaction).Commit();
        }
        async public Task<IDbCommand> ExecuteCommandAsync(string sql, IDbConnection connection, IDbTransaction transaction)
        {
            return new SqlCommand(sql, (SqlConnection)connection, (SqlTransaction)transaction);
        }
        public IDbCommand ExecuteCommand(string sql, IDbConnection connection, IDbTransaction transaction)
        {
            return new SqlCommand(sql, (SqlConnection)connection, (SqlTransaction)transaction);
        }
        async public Task<IDataReader> ExecuteReaderAsync(IDbCommand command)
        {
            return await ((SqlCommand)command).ExecuteReaderAsync();
        }
        public IDataReader ExecuteReader(IDbCommand command)
        {
            return ((SqlCommand)command).ExecuteReader();
        }
        async public Task ExecuteNonQueryAsync(IDbCommand command)
        {
            await ((SqlCommand)command).ExecuteNonQueryAsync();
        }
        public void ExecuteNonQuery(IDbCommand command)
        {
            ((SqlCommand)command).ExecuteNonQuery();
        }
        async public Task<bool> ReadAsync(IDataReader reader)
        {
            return await ((SqlDataReader)reader).ReadAsync();
        }
        public bool Read(IDataReader reader)
        {
            return ((SqlDataReader)reader).Read();
        }
        public string GetAutoIncrementWithType()
        {
            return "INT IDENTITY(1,1) PRIMARY KEY";
        }
    }
}
