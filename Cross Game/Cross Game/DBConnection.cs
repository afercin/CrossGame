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
    class DBConnection
    {
        public static int User_ID = -1;
        public static string User_NickName = "Pepe el butanero";
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
        private static bool NonQuery(string sqlQuery) => new MySqlCommand(sqlQuery, connection).ExecuteNonQuery() > 0;

        public static int CheckLogin(string email, string password, bool md5 = false)
        {
            int return_value = -1;

            if (OpenConnection())
            {
                MySqlDataReader dataReader = Query(
                    "SELECT user_id, name, number " +
                    "FROM users " +
                    "WHERE email = '" + email + "' " +
                    "AND password = '" + (md5 ? password : CreateMD5(password)) + "'"
                    );
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    User_ID = (int)dataReader["user_id"];
                    User_NickName = dataReader["name"] + "#" + (int)dataReader["number"];
                    return_value = 1;
                    dataReader.Close();
                }
                else
                    return_value = 0;

                dataReader?.Close();
                CloseConnection();
            }
            return return_value;
        }

        public static void LogOut(string LocalMAC)
        {
            NonQuery("UPDATE users SET status = 0 WHERE user_id = " + User_ID);
            NonQuery("UPDATE computers SET status = 0 WHERE MAC = " + LocalMAC);
        }

        public static string CreateMD5(string input)
        {
            StringBuilder sb = new StringBuilder();
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                
                for (int i = 0; i < hashBytes.Length; i++)
                    sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static List<string> GetMyComputers()
        {
            List<string> myComputers = new List<string>();

            if (OpenConnection())
            {
                MySqlDataReader dataReader = Query($"SELECT MAC FROM computers WHERE owner = {User_ID}");
                if (dataReader.HasRows)                
                    while (dataReader.Read())
                        myComputers.Add((string)dataReader["MAC"]);

                dataReader.Close();
                CloseConnection();
            }

            return myComputers;
        }

        public static void GetComputerData(string MAC, out string LocalIP, out string PublicIP, out int TCP, out int UDP, out string name, out int n_connections, out int max_connections, out int status)
        {
            name = LocalIP = PublicIP = null;
            TCP = UDP = n_connections = max_connections = status = -1;
            if (OpenConnection())
            {
                MySqlDataReader dataReader = Query(
                    "SELECT LocalIP, PublicIP, TCP, UDP, name, n_connections, max_connections, status " +
                    "FROM computers " +
                    "WHERE MAC = '" + MAC + "'"
                    );
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    LocalIP = (string)dataReader["LocalIP"];
                    PublicIP = (string)dataReader["PublicIP"];
                    TCP = (int)dataReader["TCP"];
                    UDP = (int)dataReader["UDP"];
                    name = (string)dataReader["name"];
                    n_connections = (int)dataReader["n_connections"];
                    max_connections = (int)dataReader["max_connections"];
                    status = (int)dataReader["status"];
                }

                dataReader.Close();
                CloseConnection();
            }
        }

        public static void SyncComputerData(string MAC, string LocalIP, string PublicIP, out int TCP, out int UDP, out string name, out int n_connections, out int max_connections, int status)
        {
            TCP = 3030;
            UDP = 3031;
            name = PublicIP;
            max_connections = 1;
            n_connections = 0;
            if (OpenConnection())
            {
                MySqlDataReader dataReader = Query(
                    "SELECT TCP, UDP, name, n_connections, max_connections " +
                    "FROM computers " +
                    "WHERE MAC = '" + MAC + "'"
                    );
                if (dataReader.HasRows) // Actualizar datos del ordenador actual
                {
                    dataReader.Read();
                    TCP = (int)dataReader["TCP"];
                    UDP = (int)dataReader["UDP"];
                    name = (string)dataReader["name"];
                    n_connections = (int)dataReader["n_connections"];
                    max_connections = (int)dataReader["max_connections"];
                    dataReader.Close();
                    NonQuery("UPDATE computers " +
                            $"SET LocalIP = '{LocalIP}', PublicIP = '{PublicIP}', status = {status} " +
                            $"WHERE MAC = '{MAC}'");
                }
                else // Insertar el nuevo equipo del usuario
                {
                    dataReader.Close();
                    NonQuery("INSERT INTO computers VALUES " +
                            $"('{MAC}', '{LocalIP}', '{PublicIP}', {TCP}, {UDP}, '{name}', {n_connections}, {max_connections}, {status}, {User_ID})");
                    //NonQuery("INSERT INTO users_computers VALUES " +
                    //        $"('{MAC}', {User_ID}, 0, null)");
                }
                CloseConnection();
            }
        }
    }
}
