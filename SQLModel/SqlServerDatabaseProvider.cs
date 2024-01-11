using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SQLModel
{
    internal class SqlServerDatabaseProvider : IDatabaseProvider
    {
        async public Task<IDbTransaction> BeginTransaction(IDbConnection connection)
        {
            return connection.BeginTransaction();
        }
        async public Task CommitTransaction(IDbTransaction transaction)
        {
            ((SqlTransaction)transaction).Commit();
        }
        async public Task<IDbCommand> ExecuteCommand(string sql, IDbConnection connection, IDbTransaction transaction)
        {
            return new SqlCommand(sql, (SqlConnection)connection, (SqlTransaction)transaction);
        }
        async public Task<IDataReader> ExecuteReader(IDbCommand command)
        {
            return await ((SqlCommand)command).ExecuteReaderAsync();
        }
        async public Task ExecuteNonQuery(IDbCommand command)
        {
            await ((SqlCommand)command).ExecuteNonQueryAsync();
        }
        async public Task<IDbConnection> OpenConnectionIternal(string connectionString)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            return conn;
        }
        async public Task CloseConnection(IDbConnection connection)
        {
            ((SqlConnection)connection).Close();
        }
        async public Task<bool> Read(IDataReader reader)
        {
            return await ((SqlDataReader)reader).ReadAsync();
        }
        public string GetAutoIncrementWithType()
        {
            return "INT IDENTITY(1,1) PRIMARY KEY";
        }
    }
}
