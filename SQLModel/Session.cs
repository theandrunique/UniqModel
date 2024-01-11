using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SQLModel
{
    public class Session : IDisposable
    {
        public Core DbCore { get { return dbcore; } }
        Core dbcore;
        IDbConnection conn;
        IDbTransaction transaction;
        public bool Expired { get { return expired; } }
        private bool expired;

        List<IDataReader> readerPool = new List<IDataReader>();
        public Session(Core dbcore)
        {
            expired = false;
            this.dbcore = dbcore;
            conn = dbcore.OpenConnection();
            try
            {
                Logging.Info($"BEGIN (implicit)");

                transaction = dbcore.BeginTransaction(conn);
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
                dbcore.CommitTransaction(transaction);
                Logging.Info($"COMMIT");
            }
            catch (Exception ex)
            {
                Logging.Info($"ROLLBACK ({ex.Message})");
                throw ex;
            }
            finally
            {
                dbcore.CloseConnection(conn);
                expired = true;
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
            catch
            {
                expired = true;
                if (dbcore.DropErrors)
                    throw;
            }
        }
        public IDataReader Execute(string query)
        {
            CheckIsExpired();
            try
            {
                IDataReader reader = dbcore.ExecuteQuery(query, conn, transaction);
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
        public void CheckIsExpired()
        {
            if (expired)
            {
                throw new Exception("The session is closed or expired due to an exception");
            }
        }
    }
}
