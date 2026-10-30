using Microsoft.Data.SqlClient;
using System.Data;

namespace WebApiCore_Ado.Services
{
    public interface IDBLayer
    {
        Task<int> ExecuteAsync(string procname, SqlParameter[] parameters);
        Task<DataTable> Table(string procname, SqlParameter[] parameters);
    }
}