using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace UniqModel
{
    public class Session : IDisposable
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
            CloseReaders();
            try
            {
                if (dbcore.AutoCommit)
                {
                    Commit();
                }
            }
            catch (Exception ex)
            {
                Logging.Info($"ROLLBACK ({ex.Message})");
                throw;
            }
            finally
            {
                dbcore.CloseConnection(conn);
                expired = true;
            }
        }
        public void Commit()
        {
            dbcore.CommitTransaction(transaction);
            expired = true;
            Logging.Info($"COMMIT");
        }
        public void Rollback()
        {
            transaction.Rollback();
            expired = true;
            Logging.Info($"ROLLBACK");
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
                if (dbcore.DropErrors)
                    throw;
            }
        }
        public IDataReader Execute(string query)
        {
            CheckIsExpired();
            try
            {
                CloseReaders();
                IDataReader reader = dbcore.ExecuteQuery(query, conn, transaction);
                readerPool.Add(reader);
                return reader;
            }
            catch
            {
                if (dbcore.DropErrors)
                    throw;
                return null;
            }
        }
        public IEnumerable<T> Query<T>(string sql, object param = null)
        {
            return conn.Query<T>(sql, param, transaction);
        }
        public T QueryFirstOrDefault<T>(string sql, object param = null)
        {
            return conn.QueryFirstOrDefault<T>(sql, param, transaction);
        }
        public void CheckIsExpired()
        {
            if (expired)
            {
                throw new Exception("The session is closed or expired due to an exception");
            }
        }
        private void CloseReaders()
        {
            foreach (IDataReader item in readerPool)
            {
                if (!item.IsClosed)
                    item.Close();
            }
            readerPool.Clear();
        }
    }
}
