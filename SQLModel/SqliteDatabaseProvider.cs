using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLModel
{
    internal class SqliteDatabaseProvider : IDatabaseProvider
    {
        public async Task<IDbTransaction> BeginTransaction(IDbConnection connection)
        {
            return ((SqliteConnection)connection).BeginTransaction();
        }

        public async Task CloseConnection(IDbConnection connection)
        {
            ((SqliteConnection)connection).Close();
        }

        public async Task CommitTransaction(IDbTransaction transaction)
        {
            ((SqliteTransaction)transaction).Commit();
        }

        public async Task<IDbCommand> ExecuteCommand(string sql, IDbConnection connection, IDbTransaction transaction)
        {
            return new SqliteCommand(sql, (SqliteConnection)connection, (SqliteTransaction)transaction);
        }

        public async Task ExecuteNonQuery(IDbCommand command)
        {
            await ((SqliteCommand)command).ExecuteNonQueryAsync();
        }

        public async Task<IDataReader> ExecuteReader(IDbCommand command)
        {
            return await ((SqliteCommand)command).ExecuteReaderAsync();
        }

        public string GetAutoIncrementWithType()
        {
            return "INTEGER PRIMARY KEY AUTOINCREMENT";
        }

        public async Task<IDbConnection> OpenConnectionIternal(string connectionString)
        {
            SqliteConnection conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();
            return conn;
        }

        public Task<bool> Read(IDataReader reader)
        {
            return ((SqliteDataReader)reader).ReadAsync();
        }
    }
}
