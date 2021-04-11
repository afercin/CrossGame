using MahApps.Metro.IconPacks;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        private bool localMachine;
        private bool editArea;
        private PackIconMaterialKind currentIcon;

        public string ComputerMAC { get => computerMAC; set => computerMAC = value; }
        public string LocalIP { get => localIP; set => localIP = value; }
        public string PublicIP { get => publicIP; set => publicIP = value; }
        public int Tcp { get => tcp; set => tcp = value; }
        public int Udp { get => udp; set => udp = value; }
        public int Status { get => status; set => status = value; }
        public int N_connections { get => n_connections; set => n_connections = value; }
        public int Max_connections { get => max_connections; set => max_connections = value; }

        public Computer(string _computerMAC = "")
        {

            InitializeComponent();

            ComputerMAC = _computerMAC;
            localMachine = string.IsNullOrEmpty(ComputerMAC);

            UpdateStatus();
        }

        private void Computer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!editArea)
            {
                UpdateStatus();
                if (n_connections < max_connections)
                    MessageBox.Show("conectandose no más");
            }
            else if (new EditComputerParams().Show(this) == true)
            {
                DBConnection.UpdateComputerInfo(this);
                ComputerName.Text += (localMachine ? " (local)" : "");
            }
            Icon.Kind = currentIcon;
            editArea = false;
        }

        public void UpdateStatus()
        {
            if (localMachine)
            {
                LocalIP = ConnectionUtils.GetLocalIPAddress();
                PublicIP = ConnectionUtils.GetPublicIPAddress();
                ComputerMAC = ConnectionUtils.GetMacByIP(LocalIP);
                Status = 1;
                DBConnection.SyncComputerData(this);
            }
            else
                DBConnection.GetComputerData(this);

            if (status != -1)
            {
                ComputerName.Text += (localMachine ? " (local)" : "");

                Connections.Text = Status == 1 ? $"{N_connections}/{Max_connections}" : "";
                if (N_connections == 0)
                {
                    switch (Status)
                    {
                        case 0:
                            ComputerBorder.BorderBrush = gray;
                            Icon.Kind = PackIconMaterialKind.MonitorOff;
                            break;
                        case 1:
                            ComputerBorder.BorderBrush = blue;
                            Icon.Kind = PackIconMaterialKind.MonitorClean;
                            break;
                    }
                }
                else if (N_connections == -1 || N_connections == Max_connections)
                {
                    Icon.Kind = PackIconMaterialKind.MonitorDashboard;
                    ComputerBorder.BorderBrush = red;
                }
                else
                {
                    Icon.Kind = PackIconMaterialKind.MonitorEye;
                    ComputerBorder.BorderBrush = yellow;
                }

                currentIcon = Icon.Kind;
            }
            else
            {
                //error
            }            
        }

        private void ComputerName_MouseEnter(object sender, MouseEventArgs e)
        {
            Icon.Kind = PackIconMaterialKind.MonitorEdit;
            editArea = true;
        }

        private void ComputerName_MouseLeave(object sender, MouseEventArgs e)
        {
            Icon.Kind = currentIcon;
            editArea = false;
        }
    }
}
