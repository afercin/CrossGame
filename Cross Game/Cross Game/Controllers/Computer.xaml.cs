using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cross_Game.Controllers
{
    /// <summary>
    /// Lógica de interacción para Computer.xaml
    /// </summary>
    public partial class Computer : UserControl
    {
        private readonly Brush red = new SolidColorBrush(Color.FromRgb(240, 30, 30));
        private readonly Brush blue = new SolidColorBrush(Color.FromRgb(50, 157, 201));
        private readonly Brush yellow = new SolidColorBrush(Color.FromRgb(240, 30, 30));
        private readonly Brush gray = new SolidColorBrush(Color.FromRgb(224, 224, 224));

        private string computerMAC;
        private string localIP;
        private string publicIP;
        private int tcp;
        private int udp;
        private int status;
        private int n_connections;
        private int max_connections;

        public string ComputerMAC
        {
            get => computerMAC;
            set => computerMAC = value;
        }
        public string LocalIP { get => localIP; }
        public string PublicIP { get => publicIP; }
        public int TCP { get => tcp; }
        public int UPD { get => udp; }
        public int Status { get => status; }
        public int NConnections { get => n_connections; }
        public int MaxConnections { get => max_connections; }

        public Computer(string _computerMAC = "")
        {
            string name;
            bool local = false;

            InitializeComponent();

            computerMAC = _computerMAC;

            if (string.IsNullOrEmpty(computerMAC))
            {
                local = true;
                localIP = ConnectionUtils.GetLocalIPAddress();
                publicIP = ConnectionUtils.GetPublicIPAddress();
                computerMAC = ConnectionUtils.GetMacByIP(LocalIP);
                status = 1;
                DBConnection.SyncComputerData(ComputerMAC, LocalIP, PublicIP, out tcp, out udp, out name, out n_connections, out max_connections, Status);
            }
            else
                DBConnection.GetComputerData(ComputerMAC, out localIP, out publicIP, out tcp, out udp, out name, out n_connections, out max_connections, out status);

            ComputerName.Text = name + (local ? " (local)" : "");
            Connections.Text = $"{n_connections}/{max_connections}";
            if (NConnections == 0)
                ComputerBorder.BorderBrush = Status == 1 ? blue : gray;
            else
                ComputerBorder.BorderBrush = NConnections == -1 || NConnections == max_connections ? red : yellow;
        }
    }
}
