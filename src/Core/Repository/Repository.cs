using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Gk.Core.Utilities;

namespace Gk.Core.Repository
{
    public interface IRepository<T> 
    {
        List<T> GetAll(string tableName);
    }
    public class Repository<T> : IRepository<T> where T : new()
    {
        public string ConnectionString { get; set; }
        public Repository()
        {
            ConnectionString = ConfigurationManager.AppSettings["DbConnection"];
        }
        public List<T> GetAll(string tableName)
        {
            //string connectionString = ConfigurationManager.AppSettings["DbConnection"];

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                // Create the command  
                SqlCommand command = new SqlCommand("SELECT * FROM " + (string.IsNullOrWhiteSpace(tableName) ? typeof(T).Name : tableName + " WITH(NOLOCK)"), conn);


                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(command))
                {
                    // create the DataSet 
                    DataSet dataSet = new DataSet();
                    // fill the DataSet using our DataAdapter 
                    dataAdapter.Fill(dataSet);

                    var list = dataSet.Tables[0].AsEnumerable()
                        .Select(dataRow => dataRow.ToObject<T>()).ToList();
                    return list;
                }
            }
        }
    }
}