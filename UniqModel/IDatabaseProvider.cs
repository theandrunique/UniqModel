using System;
using System.Data;
using System.Threading.Tasks;

namespace UniqModel
{
    public interface IDatabaseProvider
    {
        Task<IDbConnection> OpenConnectionAsync(string connectionString);
        IDbConnection OpenConnection(string connectionString);
        Task CloseConnectionAsync(IDbConnection connection);
        void CloseConnection(IDbConnection connection);
        Task<IDbCommand> ExecuteCommandAsync(string sql, IDbConnection connection, IDbTransaction transaction);
        IDbCommand ExecuteCommand(string sql, IDbConnection connection, IDbTransaction transaction);
        Task<IDbTransaction> BeginTransactionAsync(IDbConnection connection);
        IDbTransaction BeginTransaction(IDbConnection connection);
        Task CommitTransactionAsync(IDbTransaction transaction);
        void CommitTransaction(IDbTransaction transaction);
        Task<IDataReader> ExecuteReaderAsync(IDbCommand command);
        IDataReader ExecuteReader(IDbCommand command);
        Task ExecuteNonQueryAsync(IDbCommand command);
        void ExecuteNonQuery(IDbCommand command);
        Task<bool> ReadAsync(IDataReader reader);
        bool Read(IDataReader reader);
        Task CloseReaderAsync(IDataReader reader);
        string GetLastInsertRowId();
        string GetSqlType(Type type);
        string GetAutoIncrementWithType();
    }
}
