using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SQLModel
{
    internal class SqlServerDatabaseProvider : IDatabaseProvider
    {
#pragma warning disable CS1998
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
        public async Task CloseReaderAsync(IDataReader reader)
        {
#if NET48
            ((SqlDataReader)reader).Close();
#else
            await ((SqlDataReader)reader).CloseAsync();
#endif
        }
        public string GetAutoIncrementWithType()
        {
            return "INT IDENTITY(1,1) PRIMARY KEY";
        }
        public string GetSqlType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int16:
                    return "SMALLINT";
                case TypeCode.Int32:
                    return "INT";
                case TypeCode.Int64:
                    return "BIGINT";
                case TypeCode.UInt16:
                    return "SMALLINT";
                case TypeCode.UInt32:
                    return "INT";
                case TypeCode.UInt64:
                    return "BIGINT";
                case TypeCode.Single:
                    return "REAL";
                case TypeCode.Double:
                    return "FLOAT";
                case TypeCode.Decimal:
                    return "DECIMAL(18,2)";
                case TypeCode.String:
                    return "NVARCHAR(MAX)";
                case TypeCode.Boolean:
                    return "BIT";
                case TypeCode.DateTime:
                    return "DATETIME";
                default:
                    throw new ArgumentException($"Unsupported C# type: {type}");
            }

        }
    }
}
