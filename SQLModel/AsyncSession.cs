using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SQLModel
{
    public class AsyncSession : IDisposable
    {
        public Core DbCore { get { return dbcore; } }
        public IDbConnection Connection { get { return conn; } }
        public IDbTransaction Transaction { get { return transaction; } }
        Core dbcore;
        IDbConnection conn;
        IDbTransaction transaction;
        public bool Expired { get { return expired; } }
        private bool expired;

        List<IDataReader> readerPool = new List<IDataReader>();
        public AsyncSession(Core dbcore)
        {
            this.dbcore = dbcore;
        }
        async public static Task<AsyncSession> Create(Core dbcore)
        {
            var asyncSession = new AsyncSession(dbcore);

            IDbConnection conn = await dbcore.OpenConnectionAsync();
            asyncSession.conn = conn;
            try
            {
                Logging.Info($"BEGIN (implicit)");
                asyncSession.transaction = await dbcore.BeginTransactionAsync(conn);
            }
            catch (Exception ex)
            {
                Logging.Error($"Error occurred while starting the transaction. Details: {ex.Message}");
                asyncSession.expired = true;
            }
            return asyncSession;
        }
        public async void Dispose()
        {
            foreach (var reader in readerPool)
            {
                if (!reader.IsClosed)
                {
                    reader.Close();
                }
            }
            try
            {
                await dbcore.CommitTransactionAsync(transaction);
                Logging.Info($"COMMIT");
            }
            catch (Exception ex)
            {
                Logging.Info($"ROLLBACK ({ex.Message})");;
            }
            finally
            {
                await dbcore.CloseConnectionAsync(conn);
                expired = true;
            }
        }
        async public Task<T> GetById<T>(int id)
        {
            return await Crud.GetByIdAsync<T>(id, this);
        }
        async public Task Delete(object existedObject)
        {
            await Crud.DeleteAsync(existedObject, this);
        }
        async public  Task<List<T>> GetAll<T>()
        {
            return await Crud.GetAllAsync<T>(this);
        }
        async public Task Update(object existedObject)
        {
            await Crud.UpdateAsync(existedObject, this);
        }
        async public Task Add(object newObject)
        {
            await Crud.CreateAsync(newObject, this);
        }
        async public Task ExecuteNonQuery(string query)
        {
            CheckIsExpired();
            try
            {
                await dbcore.ExecuteEmptyQueryAsync(query, conn, transaction);
            }
            catch
            {
                expired = true;
                if (dbcore.DropErrors)
                    throw;
            }
        }
        async public Task<IDataReader> Execute(string query)
        {
            CheckIsExpired();
            try
            {
                IDataReader reader = await dbcore.ExecuteQueryAsync(query, conn, transaction);
                readerPool.Add(reader);
                return reader;
            }
            catch
            {
                expired = true;
                if (dbcore.DropErrors)
                    throw;
                return null;
            }
        }
        async public Task<bool> ReadAsync(IDataReader reader)
        {
            return await dbcore.ReadReaderAsync(reader);
        }
        public void CheckIsExpired()
        {
            if (expired)
            {
                throw new Exception("The session is closed or expired due to an exception");
            }
        }
    }
}
