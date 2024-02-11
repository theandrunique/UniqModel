using MySqlConnector;
using System;
using System.Data;
using System.Threading.Tasks;

namespace UniqModel.MySql
{
    public class MySqlDatabaseProvider : IDatabaseProvider
    {
        public async Task<IDbConnection> OpenConnectionAsync(string connectionString)
        {
            MySqlConnection conn = new MySqlConnection(connectionString);
            await conn.OpenAsync();
            return conn;
        }
        public IDbConnection OpenConnection(string connectionString)
        {
            MySqlConnection conn = new MySqlConnection(connectionString);
            conn.Open();
            return conn;
        }
        public async Task<IDbTransaction> BeginTransactionAsync(IDbConnection connection)
        {
            return await ((MySqlConnection)connection).BeginTransactionAsync();
        }
        public IDbTransaction BeginTransaction(IDbConnection connection)
        {
            return ((MySqlConnection)connection).BeginTransaction();
        }
        public async Task CloseConnectionAsync(IDbConnection connection)
        {
            await ((MySqlConnection)connection).CloseAsync();
        }
        public void CloseConnection(IDbConnection connection)
        {
            ((MySqlConnection)connection).Close();
        }
        public async Task CommitTransactionAsync(IDbTransaction transaction)
        {
            await ((MySqlTransaction)transaction).CommitAsync();
        }
        public void CommitTransaction(IDbTransaction transaction)
        {
            ((MySqlTransaction)transaction).Commit();
        }
        public async Task<IDbCommand> ExecuteCommandAsync(string sql, IDbConnection connection, IDbTransaction transaction)
        {
            return new MySqlCommand(sql, (MySqlConnection)connection, (MySqlTransaction)transaction);
        }
        public IDbCommand ExecuteCommand(string sql, IDbConnection connection, IDbTransaction transaction)
        {
            return new MySqlCommand(sql, (MySqlConnection)connection, (MySqlTransaction)transaction);
        }
        public async Task ExecuteNonQueryAsync(IDbCommand command)
        {
            await ((MySqlCommand)command).ExecuteNonQueryAsync();
        }
        public void ExecuteNonQuery(IDbCommand command)
        {
            ((MySqlCommand)command).ExecuteNonQuery();
        }
        public async Task<IDataReader> ExecuteReaderAsync(IDbCommand command)
        {
            return await ((MySqlCommand)command).ExecuteReaderAsync();
        }
        public IDataReader ExecuteReader(IDbCommand command)
        {
            return ((MySqlCommand)command).ExecuteReader();
        }
        public async Task<bool> ReadAsync(IDataReader reader)
        {
            return await ((MySqlDataReader)reader).ReadAsync();
        }
        public bool Read(IDataReader reader)
        {
            return ((MySqlDataReader)reader).Read();
        }
        public async Task CloseReaderAsync(IDataReader reader)
        {
#if NET48
            ((MySqlDataReader)reader).Close();
#else
            await ((MySqlDataReader)reader).CloseAsync();
#endif
        }
        public string GetLastInsertRowId()
        {
            return "SELECT LAST_INSERT_ID()";
        }
        public string GetAutoIncrementWithType()
        {
            return "INT AUTO_INCREMENT PRIMARY KEY";
        }
        public string GetSqlType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return "BOOL";

                case TypeCode.Byte:
                    return "TINYINT UNSIGNED";

                case TypeCode.SByte:
                    return "TINYINT";

                case TypeCode.Int16:
                    return "SMALLINT";

                case TypeCode.UInt16:
                    return "SMALLINT UNSIGNED";

                case TypeCode.Int32:
                    return "INT";

                case TypeCode.UInt32:
                    return "INT UNSIGNED";

                case TypeCode.Int64:
                    return "BIGINT";

                case TypeCode.UInt64:
                    return "BIGINT UNSIGNED";

                case TypeCode.Single:
                    return "FLOAT";

                case TypeCode.Double:
                    return "DOUBLE";

                case TypeCode.Decimal:
                    return "DECIMAL";

                case TypeCode.Char:
                case TypeCode.String:
                    return "VARCHAR(255)";

                case TypeCode.DateTime:
                    return "DATETIME";

                case TypeCode.Object when type == typeof(byte[]):
                    return "BLOB";

                default:
                    throw new ArgumentException($"Unsupported type: {type.Name}");
            }

        }
    }
}
