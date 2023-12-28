using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.Remoting.Messaging;

namespace SQLModel
{
    public class Session : IDisposable
    {
        Core dbcore;
        SqlConnection conn;
        SqlTransaction transaction;
        bool expired = false;

        List<SqlDataReader> readerPool = new List<SqlDataReader>();
        // static uint transactionСounter;
        public Session(Core dbcore)
        {
            this.dbcore = dbcore;

            conn = dbcore.OpenConnection();

            try
            {
                Logger.Info($"BEGIN (implicit)");

                transaction = conn.BeginTransaction();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error occurred while starting the transaction. Details: {ex.Message}");
                expired = true;
            }
        }
        public void Dispose()
        {
            foreach (var reader in readerPool)
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            try
            {
                transaction.Commit();
                Logger.Info($"COMMIT");
            }
            catch (Exception ex)
            {
                Logger.Info($"ROLLBACK ({ex.Message})");
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
                return CRUD.GetById<T>(id, this);
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
                CRUD.Delete(existedObject, this);
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
                return CRUD.GetAll<T>(conn, this);
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
                CRUD.Update(existedObject, this);
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
                CRUD.Create(newObject, this);
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
