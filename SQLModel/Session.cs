using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SQLModel
{
    public class Session : IDisposable
    {
        Core dbcore;
        SqlConnection conn;
        SqlTransaction transaction;
        public bool Expired { get { return expired; } }
        private bool expired;

        List<SqlDataReader> readerPool = new List<SqlDataReader>();
        public Session(Core dbcore)
        {
            expired = false;
            this.dbcore = dbcore;
            conn = dbcore.OpenConnection();

            try
            {
                Logging.Info($"BEGIN (implicit)");

                transaction = conn.BeginTransaction();
            }
            catch (Exception ex)
            {
                Logging.Error($"Error occurred while starting the transaction. Details: {ex.Message}");
                expired = true;
            }
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
                Logging.Info($"ROLLBACK ({ex.Message})");
                throw ex;
            }
            finally
            {
                conn.Close();
            }
        }
        public T GetById<T>(int id)
        {
            return Crud.GetById<T>(id, this);
        }
        public void Delete(object existedObject)
        {
            Crud.Delete(existedObject, this);
        }
        public List<T> GetAll<T>()
        {
            return Crud.GetAll<T>(this);
        }
        public void Update(object existedObject)
        {
            Crud.Update(existedObject, this);
        }
        public void Add(object newObject)
        {
            Crud.Create(newObject, this);
        }
        public void ExecuteNonQuery(string query)
        {
            CheckIsExpired();
            try
            {
                dbcore.ExecuteEmptyQuery(query, conn, transaction);
            }
            catch { expired = true; }
        }
        public SqlDataReader Execute(string query)
        {
            CheckIsExpired();
            try
            {
                SqlDataReader reader = dbcore.ExecuteQuery(query, conn, transaction);
                readerPool.Add(reader);
                return reader;
            }
            catch { expired = true; return null; }
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
