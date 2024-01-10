using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SQLModel
{
    public class AsyncSession : IDisposable
    {
        Core dbcore;
        SqlConnection conn;
        SqlTransaction transaction;
        public bool Expired { get { return expired; } }
        private bool expired;

        List<SqlDataReader> readerPool = new List<SqlDataReader>();
        public AsyncSession(Core dbcore)
        {
            this.dbcore = dbcore;
        }
        async public static Task<AsyncSession> Create(Core dbcore)
        {
            var asyncSession = new AsyncSession(dbcore);

            SqlConnection conn = await dbcore.OpenConnectionAsync();
            asyncSession.conn = conn;
            try
            {
                Logging.Info($"BEGIN (implicit)");

                asyncSession.transaction = conn.BeginTransaction();
            }
            catch (Exception ex)
            {
                Logging.Error($"Error occurred while starting the transaction. Details: {ex.Message}");
                asyncSession.expired = true;
            }
            return asyncSession;
        }
        public void Dispose()
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
                transaction.Commit();
                Logging.Info($"COMMIT");
            }
            catch (Exception ex)
            {
                Logging.Info($"ROLLBACK ({ex.Message})");;
            }
            finally
            {
                conn.Close();
            }
        }
        async public Task<T> GetById<T>(int id)
        {
            if (expired)
            {
                throw new Exception("The session expired due to an exception");
            }

            return await Crud.GetByIdAsync<T>(id, this);
        }
        async public Task Delete(object existedObject)
        {
            if (expired)
            {
                throw new Exception("The session expired due to an exception");
            }
            await Crud.DeleteAsync(existedObject, this);
        }
        async public  Task<List<T>> GetAll<T>()
        {
            if (expired)
            {
                throw new Exception("The session expired due to an exception");
            }
            return await Crud.GetAllAsync<T>(this);
        }
        async public Task Update(object existedObject)
        {
            if (expired)
            {
                throw new Exception("The session expired due to an exception");
            }
            await Crud.UpdateAsync(existedObject, this);
        }
        async public Task Add(object newObject)
        {
            if (expired)
            {
                throw new Exception("The session expired due to an exception");
            }
            await Crud.CreateAsync(newObject, this);
        }
        async public Task ExecuteNonQuery(string query)
        {
            try
            {
                await dbcore.ExecuteEmptyQueryAsync(query, conn, transaction);
            }
            catch { expired = true; }
        }
        async public Task<SqlDataReader> Execute(string query)
        {
            try
            {
                SqlDataReader reader = await dbcore.ExecuteQueryAsync(query, conn, transaction);
                readerPool.Add(reader);
                return reader;
            }
            catch { expired = true; return null; }
        }
    }
}
