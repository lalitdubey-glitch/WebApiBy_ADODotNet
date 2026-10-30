using Microsoft.Data;
using Microsoft.Data.SqlClient;
using System.Data;

namespace WebApiCore_Ado.Services
{
    public class DBLayer : IDBLayer
    {
        private readonly string? _conn;

        public DBLayer(IConfiguration config)
        {
            _conn = config.GetConnectionString("ConStr");
        }

        public async Task<int> ExecuteAsync(string procname, SqlParameter[] parameters)
        {
            await using SqlConnection conn = new SqlConnection(_conn);
            await using SqlCommand cmd = new SqlCommand(procname, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<DataTable> Table(string procname, SqlParameter[] parameters)
        {
            await using SqlConnection conn = new SqlConnection(_conn);
            await using SqlCommand cmd = new SqlCommand(procname, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            await conn.OpenAsync();
            DataTable dt = new DataTable();
            using var reader = await cmd.ExecuteReaderAsync();
            dt.Load(reader);
            return dt;
        }
    }
}
