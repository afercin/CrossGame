using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross_Game
{
    public class ComputerData
    {
        private string mac;
        private string localIP;
        private string publicIP;
        private string name;
        private int fps;
        private int tcp;
        private int udp;
        private int status;
        private int n_connections;
        private int max_connections;


        public string MAC { get => mac; set => mac = value; }
        public string LocalIP { get => localIP; set => localIP = value; }
        public string PublicIP { get => publicIP; set => publicIP = value; }
        public int Tcp { get => tcp; set => tcp = value; }
        public int Udp { get => udp; set => udp = value; }
        public int Status { get => status; set => status = value; }
        public int N_connections { get => n_connections; set => n_connections = value; }
        public int Max_connections { get => max_connections; set => max_connections = value; }
        public string Name { get => name; set => name = value; }
        public int FPS { get => fps; set => fps = value; }
    }
}
