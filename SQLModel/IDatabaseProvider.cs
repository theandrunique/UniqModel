using System.Data;
using System.Threading.Tasks;

namespace SQLModel
{
    public interface IDatabaseProvider
    {
        Task<IDbConnection> OpenConnectionIternal(string connectionString);
        Task CloseConnection(IDbConnection connection);
        Task<IDbCommand> ExecuteCommand(string sql, IDbConnection connection, IDbTransaction transaction);
        Task<IDbTransaction> BeginTransaction(IDbConnection connection);
        Task CommitTransaction(IDbTransaction transaction);
        Task<IDataReader> ExecuteReader(IDbCommand command);
        Task ExecuteNonQuery(IDbCommand command);
        Task<bool> Read(IDataReader reader);
    }
}
