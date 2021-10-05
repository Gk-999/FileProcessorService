using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gk.Core.Utilities
{
    public class Sql
    {
        public static int ExecuteNonQueryWithStoredProcedure(string connectionString, string sql, int commandTimeout = 30)
        {
            return ExecuteNonQueryWithStoredProcedure(connectionString, sql, new List<SqlParameter>(), commandTimeout);
        }

        public static int ExecuteNonQueryWithStoredProcedure(string connectionString, string sql, IEnumerable<SqlParameter> parameters, int commandTimeout = 30)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(sql, connection);

                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = commandTimeout;
                command.Parameters.AddRange(parameters.ToArray());
                connection.Open();
                return command.ExecuteNonQuery();
            }
        }
    }
}
