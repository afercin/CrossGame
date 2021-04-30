using Cross_Game.Controllers;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Cross_Game
{
    class DBConnection
    {
        private static MySqlConnection connection = new MySqlConnection(
            "SERVER=crossgame.sytes.net;" +
            "DATABASE=CrossGame;" +
            "UID=client;" +
            "PASSWORD=Patata_123;"
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
                LogUtils.AppendLogHeader(LogUtils.DatabaseErrorsLog);
                switch (ex.Number)
                {
                    case 0:
                        LogUtils.AppendLogError(LogUtils.DatabaseErrorsLog, "Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        LogUtils.AppendLogError(LogUtils.DatabaseErrorsLog, "Invalid username/password, please try again");
                        break;

                    default:
                        LogUtils.AppendLogError(LogUtils.DatabaseErrorsLog, ex.Message);
                        break;

                }
                LogUtils.AppendLogFooter(LogUtils.DatabaseErrorsLog);
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
                LogUtils.AppendLogHeader(LogUtils.DatabaseErrorsLog);
                LogUtils.AppendLogError(LogUtils.DatabaseErrorsLog, ex.Message);
                LogUtils.AppendLogFooter(LogUtils.DatabaseErrorsLog);
                return false;
            }
        }

        private static MySqlDataReader Query(string sqlQuery) => new MySqlCommand(sqlQuery, connection).ExecuteReader();

        private static bool NonQuery(string sqlQuery) => new MySqlCommand(sqlQuery, connection).ExecuteNonQuery() > 0;

        public static UserData CheckLogin(string email, string password, bool md5 = false)
        {
            UserData currentUser = null;

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
                    currentUser = new UserData((int)dataReader["user_id"],
                                           (string)dataReader["name"],
                                           (int)dataReader["number"]);
                    dataReader.Close();
                }
                else
                    currentUser = new UserData(0, "0", 0);

                dataReader?.Close();
                CloseConnection();
            }
            return currentUser;
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

        public static List<string> GetMyComputers(UserData currentUser)
        {
            List<string> myComputers = new List<string>();

            if (OpenConnection())
            {
                MySqlDataReader dataReader = Query($"SELECT MAC FROM computers WHERE owner = {currentUser.ID}");
                if (dataReader.HasRows)                
                    while (dataReader.Read())
                        myComputers.Add((string)dataReader["MAC"]);

                dataReader.Close();
                CloseConnection();
            }

            return myComputers;
        }

        internal static void SyncLocalMachinerData(UserData currentUser)
        {
            currentUser.localMachine.Tcp = 3030;
            currentUser.localMachine.Udp = 3031;
            currentUser.localMachine.Name = currentUser.localMachine.PublicIP;
            currentUser.localMachine.Max_connections = 1;
            currentUser.localMachine.N_connections = 0;
            if (OpenConnection())
            {
                MySqlDataReader dataReader = Query(
                    "SELECT TCP, UDP, name, n_connections, max_connections " +
                    "FROM computers " +
                    "WHERE MAC = '" + currentUser.localMachine.MAC + "'"
                    );
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    currentUser.localMachine.Tcp = (int)dataReader["TCP"];
                    currentUser.localMachine.Udp = (int)dataReader["UDP"];
                    currentUser.localMachine.Name = (string)dataReader["name"];
                    currentUser.localMachine.N_connections = (int)dataReader["n_connections"];
                    currentUser.localMachine.Max_connections = (int)dataReader["max_connections"];

                    dataReader.Close();

                    NonQuery("UPDATE computers " +
                            $"SET LocalIP = '{currentUser.localMachine.LocalIP}', PublicIP = '{currentUser.localMachine.PublicIP}', status = {currentUser.localMachine.Status} " +
                            $"WHERE MAC = '{currentUser.localMachine.MAC}'");
                }
                else
                {
                    dataReader.Close();

                    NonQuery("INSERT INTO computers VALUES " +
                            $"('{currentUser.localMachine.MAC}', '{currentUser.localMachine.LocalIP}', '{currentUser.localMachine.PublicIP}', {currentUser.localMachine.Tcp}, {currentUser.localMachine.Udp}, " +
                            $"'{currentUser.localMachine.Name}', {currentUser.localMachine.N_connections}, {currentUser.localMachine.Max_connections}, {currentUser.localMachine.Status}, {currentUser.ID})");
                }
                NonQuery($"UPDATE computers " +
                            $"SET LocalIP = '{currentUser.localMachine.LocalIP}', PublicIP = '{currentUser.localMachine.PublicIP}', status = {currentUser.localMachine.Status} " +
                            $"WHERE MAC = '{currentUser.localMachine.MAC}'");
                CloseConnection();
            }
        }

        internal static void GetComputerData(ComputerData pc)
        {
            pc.Status = -1;
            if (OpenConnection())
            {
                MySqlDataReader dataReader = Query(
                    "SELECT LocalIP, PublicIP, TCP, UDP, name, n_connections, max_connections, status " +
                    "FROM computers " +
                    "WHERE MAC = '" + pc.MAC + "'"
                    );
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    pc.LocalIP = (string)dataReader["LocalIP"];
                    pc.PublicIP = (string)dataReader["PublicIP"];
                    pc.Tcp = (int)dataReader["TCP"];
                    pc.Udp = (int)dataReader["UDP"];
                    pc.Name = (string)dataReader["name"];
                    pc.N_connections = (int)dataReader["n_connections"];
                    pc.Max_connections = (int)dataReader["max_connections"];
                    pc.Status = (int)dataReader["status"];
                }

                dataReader.Close();
                CloseConnection();
            }
        }

        public static void UpdateComputerInfo(Computer computer)
        {
            if (OpenConnection())
            {
                NonQuery("UPDATE computers " +
                        $"SET TCP = {computer.pc.Tcp}, UDP = {computer.pc.Udp}, name = '{computer.ComputerName.Text}', max_connections = '{computer.pc.Max_connections}' " +
                        $"WHERE MAC = '{computer.pc.MAC}'");
                CloseConnection();
            }
        }

        public static void LogOut(UserData currentUser)
        {
            if (OpenConnection())
            {
                NonQuery($"UPDATE users SET status = 0 WHERE user_id = {currentUser.ID}");
                NonQuery($"UPDATE computers SET status = 0, n_connections = 0 WHERE MAC = '{currentUser.localMachine.MAC}'");
                CloseConnection();
            }
        }
    }
}
