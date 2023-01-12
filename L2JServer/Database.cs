using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Diagnostics;

namespace L2JServer
{
    public static class Database
    {

        public static SqlConnection GetDBConnection()
        {
            string cnString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\phili\\source\\repos\\L2JChat\\L2JServer\\userDB.mdf;Integrated Security=True";
            SqlConnection cnConnection = new SqlConnection(cnString);
            if (cnConnection.State != ConnectionState.Open) cnConnection.Open();
            return cnConnection;

        }

        public static DataTable GetDataTable(string sqlText)
        {
            SqlConnection cnConnection = GetDBConnection();
            DataTable dt = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter(sqlText, cnConnection);
            adapter.Fill(dt);
            return dt;
        }

        public static void ExecuteSQL(string sqlText)
        {

            SqlConnection cnConnection = GetDBConnection();
            SqlCommand cmdCommand = new SqlCommand(sqlText, cnConnection);
            cmdCommand.ExecuteNonQuery();
        }

        public static void CloseDBConnection()
        {
            string cnString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\phili\\source\\repos\\L2JChat\\L2JServer\\userDB.mdf;Integrated Security=True";
            SqlConnection cnConnection = new SqlConnection(cnString);
            if (cnConnection.State != ConnectionState.Closed) cnConnection.Close();
        }

        public static string DisplaySqlErrors(SqlException exception)
        {
            for (int i = 0; i < exception.Errors.Count; i++)
            {
                Debug.WriteLine("Index #" + i + "\n" +
                    "Error: " + exception.Errors[i].ToString() + "\n");

            }
            string lastE = "Index #" + exception.Errors.Count + "\n" +
                    "Error: " + exception.Errors[exception.Errors.Count].ToString() + "\n";
            return lastE;
        }
    }
}
