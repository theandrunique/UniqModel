using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace UniqModel
{
    public class AsyncSession : IDisposable
    {
        public IDbConnection Connection { get { return _conn; } }
        public IDbTransaction Transaction { get { return _transaction; } }
        IDbConnection _conn;
        IDbTransaction _transaction;
        public bool Expired { get { return _expired; } }
        private bool _expired;

        List<IDataReader> _readerPool = new List<IDataReader>();
        public AsyncSession() { }
        async public static Task<AsyncSession> Create()
        {
            var asyncSession = new AsyncSession();

            asyncSession._conn = await CoreImpl.OpenConnectionAsync();
            try
            {
                Logging.Info($"BEGIN (implicit)");
                asyncSession._transaction = await CoreImpl.BeginTransactionAsync(asyncSession._conn);
            }
            catch (Exception ex)
            {
                Logging.Error($"Error occurred while starting the transaction. Details: {ex.Message}");
                asyncSession._expired = true;
                if (UniqSettings.DropErrors)
                    throw;
                return null;
            }
            return asyncSession;
        }
        public async void Dispose()
        {
            await CloseReaders();
            try
            {
                if (UniqSettings.AutoCommit)
                {
                    await Commit();
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
                    await CoreImpl.CloseConnectionAsync(_conn);
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
        public async Task Commit()
        {
            await CoreImpl.CommitTransactionAsync(_transaction);
            Logging.Info($"COMMIT");
        }
        public async Task RollBack()
        {
            _transaction.Rollback();
        }
        async public Task<T> GetById<T>(int id)
        {
            return await Crud.GetByIdAsync<T>(id, this);
        }
        async public Task Delete(object existedObject)
        {
            await Crud.DeleteAsync(existedObject, this);
        }
        async public Task<List<T>> GetAll<T>()
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
        async public Task Execute(string query, object param = null)
        {
            try
            {
                await CoreImpl.ExecuteAsync(query, param, _conn, _transaction);
            }
            catch
            {
                if (UniqSettings.DropErrors)
                    throw;
            }
        }
        async public Task<IDataReader> ExecuteReader(string query)
        {
            try
            {
                await CloseReaders();
                IDataReader reader = await CoreImpl.ExecuteQueryAsync(query, _conn, _transaction);
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
        async public Task<bool> ReadAsync(IDataReader reader)
        {
            return await CoreImpl.ReadReaderAsync(reader);
        }
        private async Task CloseReaders()
        {
            foreach (IDataReader item in _readerPool)
            {
                if (!item.IsClosed)
                    await CoreImpl.CloseReaderAsync(item);
            }
            _readerPool.Clear();
        }
    }
}
