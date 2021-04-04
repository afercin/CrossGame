using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Cross_Game
{
    class DBConnect
    {
        private static MySqlConnection connection = new MySqlConnection(
            "SERVER=localhost;" +
            "DATABASE=CrossGame;" +
            "UID=root;" +
            "PASSWORD=;"
        );
        
        private static bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        Console.WriteLine("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        Console.WriteLine("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }
        
        private static bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /*
        public void Delete()
        {
            string query = "DELETE FROM tableinfo WHERE name='John Smith'";

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }
        */
        private static MySqlDataReader Query(string sqlQuery) => new MySqlCommand(sqlQuery, connection).ExecuteReader();

        public static int CheckLogin(string email, string password)
        {
            int return_value = -1;

            if (OpenConnection())
            {
                MD5 md5 = MD5.Create();
                MySqlDataReader dataReader = Query(
                    "SELECT user_id " +
                    "FROM users " +
                    "WHERE email = '" + email + "' " +
                    "AND password = '" + CreateMD5(password) + "'"
                    );
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    return_value = (int)dataReader["user_id"];
                }
                else
                    return_value = 0;

                dataReader.Close();
                CloseConnection();
            }
            return return_value;
        }

        private static string CreateMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}
