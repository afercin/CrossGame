using Cross_Game.DataManipulation;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace Cross_Game
{
    class DBConnection
    {
        private static string UserEmail;
        private static string UserPassword;
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

        public static UserData CheckLogin(string email, string password, bool overrideCredentials = true)
        {
            UserData currentUser = null;

            if (OpenConnection())
            {
                password = Crypto.CreateSHA256(password);
                MySqlDataReader dataReader = Query($"CALL GetUserName('{email}', '{password}');");

                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    currentUser = new UserData((string)dataReader["name"],
                                               (int)dataReader["number"]);
                    if (overrideCredentials)
                    {
                        UserEmail = email;
                        UserPassword = password;
                    }
                }
                else
                    currentUser = new UserData(string.Empty, 0);

                dataReader.Close();
                CloseConnection();
            }
            return currentUser;
        }

        public static bool IsCorretIP(string email, string password, string remoteMachineMAC, string remoteLocalIP, string remotePublicIP)
        {
            bool isCorrect = false;
            if (OpenConnection())
            {
                MySqlDataReader dataReader = Query($"CALL GetComputerIP('{UserEmail}', '{UserPassword}', '{remoteMachineMAC}');");
                if (dataReader.HasRows)
                    isCorrect = (string)dataReader["LocalIP"] == remoteLocalIP && (string)dataReader["PublicIP"] == remotePublicIP;

                dataReader.Close();
                CloseConnection();
            }
            return isCorrect;
        }

        public static int GetUserPriority(string email, string password, string localMachineMAC)
        {
            int priority = 0;
            var list = GetUserComputers(email, password);

            if (list.Contains(localMachineMAC))
                priority = 2;
            else
            {
                list = GetSharedComputers(email, password);
                if (list.Contains(localMachineMAC))
                    priority = 1;
            }
            return priority;
        }

        public static List<string> GetUserComputers(string email = null, string password = null) => GetComputers("GetUserComputers", email, password);

        public static List<string> GetSharedComputers(string email = null, string password = null) => GetComputers("GetSharedComputers", email, password);

        private static List<string> GetComputers(string procedure, string email = null, string password = null)
        {
            if (!ConnectionUtils.HasInternetConnection())
                return null;

            List<string> computers = new List<string>();

            if (OpenConnection())
            {
                if (email == null)
                {
                    email = UserEmail;
                    password = UserPassword;
                }

                MySqlDataReader dataReader = Query($"CALL {procedure}('{UserEmail}', '{UserPassword}');");
                if (dataReader.HasRows)
                    while (dataReader.Read())
                        computers.Add((string)dataReader["MAC"]);

                dataReader.Close();
                CloseConnection();
            }
            return computers;
        }

        public static ComputerData GetComputerData(string MAC)
        {
            ComputerData computer = new ComputerData(MAC);
            computer.Status = -1;
            if (OpenConnection())
            {
                MySqlDataReader dataReader = Query($"CALL GetComputerData('{UserEmail}', '{UserPassword}', '{MAC}');");
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    computer.LocalIP = (string)dataReader["LocalIP"];
                    computer.PublicIP = (string)dataReader["PublicIP"];
                    computer.Tcp = (int)dataReader["TCP"];
                    computer.Udp = (int)dataReader["UDP"];
                    computer.Name = (string)dataReader["name"];
                    computer.N_connections = (int)dataReader["n_connections"];
                    computer.Max_connections = (int)dataReader["max_connections"];
                    computer.Status = (int)dataReader["status"];
                    computer.FPS = (int)dataReader["FPS"];
                }

                dataReader.Close();
                CloseConnection();
            }
            return computer;
        }

        public static bool AddComputer(ComputerData computer)
        {
            bool added = false;
            if (OpenConnection())
            {
                MySqlDataReader dataReader = Query($"CALL AddComputer('{UserEmail}', '{UserPassword}', '{computer.MAC}', '{computer.LocalIP}', '{computer.PublicIP}', '{computer.Name}');");

                dataReader.Read();
                added = (int)dataReader["result"] == 1;

                dataReader.Close();
                CloseConnection();
            }
            return added;
        }

        public static void UpdateTransmissionConf(ComputerData computer)
        {
            if (OpenConnection())
            {
                NonQuery($"CALL UpdateTransmissionConf('{UserEmail}', '{UserPassword}', '{computer.MAC}', {computer.Tcp}, {computer.Udp}, '{computer.Name}', {computer.Max_connections});");
                CloseConnection();
            }
        }

        public static void UpdateComputerStatus(ComputerData computer)
        {
            if (OpenConnection())
            {
                NonQuery($"CALL UpdateComputerStatus('{UserEmail}', '{UserPassword}', '{computer.MAC}', '{computer.LocalIP}', '{computer.PublicIP}', {computer.N_connections}, {computer.Status});");
                CloseConnection();
            }
        }

        public static void LogOut(ComputerData computer)
        {
            computer.Status = 0;
            computer.N_connections = 0;
            UpdateComputerStatus(computer);
            //if (OpenConnection())
            //{
            //    NonQuery($"CALL LogOut('{UserEmail}', '{UserPassword}');");
            //    CloseConnection();
            //}
        }
    }
}
