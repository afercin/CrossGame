namespace Cross_Game
{
    public class UserData
    {
        public int ID { get; private set; }
        public string Name { get; private set; }
        public int Number { get; private set; }

        public ComputerData localMachine { get; set; }

        public UserData(int id, string name, int number)
        {
            ID = id;
            Name = name;
            Number = number;
            localMachine = null;
        }

        public void SyncLocalMachine()
        {
            ConnectionUtils.GetComputerNetworkInfo(out string localIP, out string publicIP, out string macAddress);
            localMachine = new ComputerData()
            {
                LocalIP = localIP,
                PublicIP = publicIP,
                MAC = macAddress
            };
            DBConnection.SyncLocalMachinerData(this);
        }
    }
}
