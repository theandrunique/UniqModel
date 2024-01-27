using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace UniqModel
{
    public class Session : IDisposable
    {
        public IDbConnection Connection { get { return _conn; } }
        public IDbTransaction Transaction { get { return _transaction; } }
        IDbConnection _conn;
        IDbTransaction _transaction;
        public bool Expired { get { return _expired; } }
        private bool _expired;

        List<IDataReader> _readerPool = new List<IDataReader>();
        public Session()
        {
            _expired = false;
            _conn = CoreImpl.OpenConnection();
            try
            {
                Logging.Info($"BEGIN (implicit)");

                _transaction = CoreImpl.BeginTransaction(_conn);
            }
            catch (Exception ex)
            {
                Logging.Error($"Error occurred while starting the transaction. Details: {ex.Message}");
                _expired = true;
                if (UniqSettings.DropErrors)
                    throw;
            }
        }
        public void Dispose()
        {
            CloseReaders();
            try
            {
                if (UniqSettings.AutoCommit)
                {
                    Commit();
                }
            }
            catch (Exception ex)
            {
                Logging.Info($"ROLLBACK ({ex.Message})");
                if (UniqSettings.DropErrors)
                    throw;
            }
            finally
            {
                try
                {
                    CoreImpl.CloseConnection(_conn);
                }
                catch (Exception ex)
                {
                    Logging.Error($"Error occurred while closing the connection. Details: {ex.Message}");
                    if (UniqSettings.DropErrors)
                        throw;
                }
                _expired = true;
            }
        }
        public void Commit()
        {
            CoreImpl.CommitTransaction(_transaction);
            _expired = true;
            Logging.Info($"COMMIT");
        }
        public void Rollback()
        {
            _transaction.Rollback();
            _expired = true;
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
        public void Execute(string query, object param = null)
        {
            try
            {
                CoreImpl.Execute(query, param, _conn, _transaction);
            }
            catch
            {
                if (UniqSettings.DropErrors)
                    throw;
            }
        }
        public IDataReader Execute(string query)
        {
            try
            {
                CloseReaders();
                IDataReader reader = CoreImpl.ExecuteQuery(query, _conn, _transaction);
                _readerPool.Add(reader);
                return reader;
            }
            catch
            {
                if (UniqSettings.DropErrors)
                    throw;
                return null;
            }
        }
        public IEnumerable<T> Query<T>(string sql, object param = null)
        {
            return CoreImpl.Query<T>(_conn, sql, param, _transaction);
        }
        public T QueryFirstOrDefault<T>(string sql, object param = null)
        {
            return CoreImpl.QueryFirstOrDefault<T>(_conn, sql, param, _transaction);
        }
        private void CloseReaders()
        {
            foreach (IDataReader item in _readerPool)
            {
                if (!item.IsClosed)
                    item.Close();
            }
            _readerPool.Clear();
        }
    }
}
