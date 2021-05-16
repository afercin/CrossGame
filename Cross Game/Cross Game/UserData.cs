using System;

namespace Cross_Game
{
    public class UserData
    {
        public string Name { get; private set; }
        public int Number { get; private set; }

        public ComputerData localMachine { get; set; }

        public UserData(string name, int number)
        {
            Name = name;
            Number = number;
            localMachine = null;
        }

        public void SyncLocalMachine()
        {
            ConnectionUtils.GetComputerNetworkInfo(out string localIP, out string publicIP, out string macAddress);
            localMachine = DBConnection.GetComputerData(macAddress);
            localMachine.LocalIP = localIP;
            localMachine.PublicIP = publicIP;
            if (localMachine.Status != -1)
            {
                localMachine.N_connections = 0;
                localMachine.Status = 1;
                DBConnection.UpdateComputerStatus(localMachine);
            }
            else
            {
                localMachine.Name = Environment.MachineName;
                DBConnection.AddComputer(localMachine);
            }
        }
    }
}
