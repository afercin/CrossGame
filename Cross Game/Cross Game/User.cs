using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross_Game
{
    class User
    {
        public int ID { get; private set; }
        public string Name { get; private set; }
        public int Number { get; private set; }

        public PC localMachine { get; set; }

        public User(int id, string name, int number)
        {
            ID = id;
            Name = name;
            Number = number;
            localMachine = null;
        }

        public void SyncLocalMachine()
        {
            ConnectionUtils.GetComputerNetworkInfo(out string localIP, out string publicIP, out string macAddress);
            localMachine = new PC()
            {
                LocalIP = localIP,
                PublicIP = publicIP,
                MAC = macAddress
            };
            DBConnection.SyncLocalMachinerData(localMachine);
        }
    }
}
