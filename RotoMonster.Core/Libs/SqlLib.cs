using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace RotoMonster.Core.Libs
{
    public class SqlLib
    {
        private readonly string connectionString;

        public SqlLib(string connectionString)
        {
            // "Data Source=.\\SQL2019; Initial Catalog=R_NBA; Integrated Security=true;"
            this.connectionString = connectionString;
        }

        public DataTable ExecuteSelect(SqlCommand cmd)
        {
            DataTable table = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    table = ds.Tables[0];
                }
                catch (SqlException ex)
                {
                    throw ex;
                }
                finally
                {
                    conn.Close();
                }
            }

            return table;
        }
        
        public int ExecuteNonSelect(SqlCommand cmd)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {
                conn.Open();
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;
                return cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
        }

    }
}
