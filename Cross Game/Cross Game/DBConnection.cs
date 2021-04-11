using Cross_Game.Controllers;
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


        internal static void GetComputerData(Computer computer)
        {
            computer.Status = -1;
            if (OpenConnection())
            {
                MySqlDataReader dataReader = Query(
                    "SELECT LocalIP, PublicIP, TCP, UDP, name, n_connections, max_connections, status " +
                    "FROM computers " +
                    "WHERE MAC = '" + computer.ComputerMAC + "'"
                    );
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    computer.LocalIP = (string)dataReader["LocalIP"];
                    computer.PublicIP = (string)dataReader["PublicIP"];
                    computer.Tcp = (int)dataReader["TCP"];
                    computer.Udp = (int)dataReader["UDP"];
                    computer.ComputerName.Text = (string)dataReader["name"];
                    computer.N_connections = (int)dataReader["n_connections"];
                    computer.Max_connections = (int)dataReader["max_connections"];
                    computer.Status = (int)dataReader["status"];
                }

                dataReader.Close();
                CloseConnection();
            }
        }

        internal static void SyncComputerData(Computer computer)
        {
            computer.Tcp = 3030;
            computer.Udp = 3031;
            computer.ComputerName.Text = computer.PublicIP;
            computer.Max_connections = 1;
            computer.N_connections = 0;
            if (OpenConnection())
            {
                MySqlDataReader dataReader = Query(
                    "SELECT TCP, UDP, name, n_connections, max_connections " +
                    "FROM computers " +
                    "WHERE MAC = '" + computer.ComputerMAC + "'"
                    );
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    computer.Tcp = (int)dataReader["TCP"];
                    computer.Udp = (int)dataReader["UDP"];
                    computer.ComputerName.Text = (string)dataReader["name"];
                    computer.N_connections = (int)dataReader["n_connections"];
                    computer.Max_connections = (int)dataReader["max_connections"];

                    dataReader.Close();

                    NonQuery("UPDATE computers " +
                            $"SET LocalIP = '{computer.LocalIP}', PublicIP = '{computer.PublicIP}', status = {computer.Status} " +
                            $"WHERE MAC = '{computer.ComputerMAC}'");
                }
                else
                {
                    dataReader.Close();

                    NonQuery("INSERT INTO computers VALUES " +
                            $"('{computer.ComputerMAC}', '{computer.LocalIP}', '{computer.PublicIP}', {computer.Tcp}, {computer.Udp}, " +
                            $"'{computer.ComputerName.Text}', {computer.N_connections}, {computer.Max_connections}, {computer.Status}, {User_ID})");
                }
                CloseConnection();
            }
        }

        internal static void UpdateComputerInfo(Computer computer)
        {
            if (OpenConnection())
            {
                NonQuery("UPDATE computers " +
                        $"SET TCP = {computer.Tcp}, UDP = {computer.Udp}, name = '{computer.ComputerName.Text}', max_connections = '{computer.Max_connections}' " +
                        $"WHERE MAC = '{computer.ComputerMAC}'");
                CloseConnection();
            }
        }
    }
}
