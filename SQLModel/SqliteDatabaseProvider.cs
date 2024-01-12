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
        {
            return ((SqliteConnection)connection).BeginTransaction();
        }
        public IDbTransaction BeginTransaction(IDbConnection connection)
        {
            return ((SqliteConnection)connection).BeginTransaction();
        }
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
        public string GetAutoIncrementWithType()
        {
            return "INTEGER PRIMARY KEY AUTOINCREMENT";
        }
    }
}
