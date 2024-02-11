using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Threading.Tasks;

namespace UniqModel.Sqlite
{
    public class SqliteDatabaseProvider : IDatabaseProvider
    {
#pragma warning disable CS1998
        public async Task<IDbConnection> OpenConnectionAsync(string connectionString)
        {
            SqliteConnection conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();
            return conn;
        }
        public IDbConnection OpenConnection(string connectionString)
        {
            SqliteConnection conn = new SqliteConnection(connectionString);
            conn.Open();
            return conn;
        }
        public async Task<IDbTransaction> BeginTransactionAsync(IDbConnection connection)
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
        {
            return ((SqliteConnection)connection).BeginTransaction();
        }
        public IDbTransaction BeginTransaction(IDbConnection connection)
        {
            return ((SqliteConnection)connection).BeginTransaction();
        }
#pragma warning disable CS1998
        public async Task CloseConnectionAsync(IDbConnection connection)
        {
            ((SqliteConnection)connection).Close();
        }
        public void CloseConnection(IDbConnection connection)
        {
            ((SqliteConnection)connection).Close();
        }
        public async Task CommitTransactionAsync(IDbTransaction transaction)
        {
            ((SqliteTransaction)transaction).Commit();
        }
        public void CommitTransaction(IDbTransaction transaction)
        {
            ((SqliteTransaction)transaction).Commit();
        }
        public async Task<IDbCommand> ExecuteCommandAsync(string sql, IDbConnection connection, IDbTransaction transaction)
        {
            return new SqliteCommand(sql, (SqliteConnection)connection, (SqliteTransaction)transaction);
        }
        public IDbCommand ExecuteCommand(string sql, IDbConnection connection, IDbTransaction transaction)
        {
            return new SqliteCommand(sql, (SqliteConnection)connection, (SqliteTransaction)transaction);
        }
        public async Task ExecuteNonQueryAsync(IDbCommand command)
        {
            await ((SqliteCommand)command).ExecuteNonQueryAsync();
        }
        public void ExecuteNonQuery(IDbCommand command)
        {
            ((SqliteCommand)command).ExecuteNonQuery();
        }
        public async Task<IDataReader> ExecuteReaderAsync(IDbCommand command)
        {
            return await ((SqliteCommand)command).ExecuteReaderAsync();
        }
        public IDataReader ExecuteReader(IDbCommand command)
        {
            return ((SqliteCommand)command).ExecuteReader();
        }
        public async Task<bool> ReadAsync(IDataReader reader)
        {
            return await ((SqliteDataReader)reader).ReadAsync();
        }
        public bool Read(IDataReader reader)
        {
            return ((SqliteDataReader)reader).Read();
        }
        public async Task CloseReaderAsync(IDataReader reader)
        {
#if NET48
            ((SqliteDataReader)reader).Close();
#else
            await ((SqliteDataReader)reader).CloseAsync();
#endif
        }
        public string GetLastInsertRowId()
        {
            return "SELECT last_insert_rowid()";
        }
        public string GetAutoIncrementWithType()
        {
            return "INTEGER PRIMARY KEY AUTOINCREMENT";
        }
        public string GetSqlType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return "INTEGER";

                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return "INTEGER";

                case TypeCode.Object when type == typeof(byte[]):
                    return "BLOB";

                case TypeCode.DateTime:
                case TypeCode.String when type == typeof(DateTimeOffset):
                case TypeCode.String when type == typeof(TimeSpan):
                case TypeCode.Decimal:
                case TypeCode.String when type == typeof(Guid):
                    return "TEXT";

                case TypeCode.Char:
                case TypeCode.String:
                    return "TEXT";

                case TypeCode.Single:
                case TypeCode.Double:
                    return "REAL";

                default:
                    throw new ArgumentException($"Unsupported type: {type.Name}");
            }

        }
    }
}
