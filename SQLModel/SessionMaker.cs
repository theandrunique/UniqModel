using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SQLModel
{
    internal class SessionMaker : CRUD, IDisposable
    {
        Core dbcore;
        SqlConnection conn;
        SqlTransaction transaction;
        public SessionMaker(Core dbcore)
        {
            this.dbcore = dbcore;
            if (!dbcore.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated. Access denied.");

            conn = dbcore.OpenConnection();
            transaction = conn.BeginTransaction();

        }
        public void Dispose()
        {
            try
            {
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                conn.Close();
            }
        }
        public T GetById<T>(int id)
        {
            return CRUD.GetById<T>(id, conn);
        }
        public void Delete(object existedObject)
        {
            CRUD.Delete(existedObject, conn);
        }
        public List<T> GetAll<T>(int id)
        {
            return CRUD.GetAll<T>(conn);
        }
        public void Update(object existedObject)
        {
            CRUD.Update(existedObject, conn);
        }
        public void Add(object newObject)
        {
            CRUD.Create(newObject, conn);
        }
    }
}
