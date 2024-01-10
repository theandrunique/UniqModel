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
                // transaction.Rollback();
                // throw;
            }
            finally
            {
                conn.Close();
            }
        }
        public T GetById<T>(int id)
        {
            if (!expired)
            {
                return Crud.GetById<T>(id, this);
            }
            else
            {
                throw new Exception("The session expired due to an exception");
            }
        }
        public void Delete(object existedObject)
        {
            if (!expired)
            {
                Crud.Delete(existedObject, this);
            }
            else
            {
                throw new Exception("The session expired due to an exception");
            }
        }
        public List<T> GetAll<T>()
        {
            if (!expired)
            {
                return Crud.GetAll<T>(this);
            }
            else
            {
                throw new Exception("The session expired due to an exception");
            }
        }
        public void Update(object existedObject)
        {
            if (!expired)
            {
                Crud.Update(existedObject, this);
            }
            else
            {
                throw new Exception("The session expired due to an exception");
            }
        }
        public void Add(object newObject)
        {
            if (!expired)
            {
                Crud.Create(newObject, this);
            }
            else
            {
                throw new Exception("The session expired due to an exception");
            }
        }
        public void ExecuteNonQuery(string query)
        {
            try
            {
                dbcore.ExecuteEmptyQuery(query, conn, transaction);
            }
            catch { expired = true; }
        }
        public SqlDataReader Execute(string query)
        {
            try
            {
                SqlDataReader reader = dbcore.ExecuteQuery(query, conn, transaction);
                readerPool.Add(reader);
                return reader;
            }
            catch { expired = true; return null; }
        }
    }
}
