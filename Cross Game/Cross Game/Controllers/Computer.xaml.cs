using MahApps.Metro.IconPacks;
using RTDP;
using System;
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
        public event EventHandler ComputerClicked;

        public ComputerData pc;

        public Computer(string mac)
        {
            InitializeComponent();

            UpdateStatus(mac);
        }

        private void Computer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            UpdateStatus();
            if (pc.N_connections < pc.Max_connections && pc.Status > 0 || pc.Status == -1)
                ComputerClicked.Invoke(pc, new EventArgs());
        }

        public void UpdateStatus(string mac = null)
        {
            pc = DBConnection.GetComputerData(mac != null ? mac : pc.MAC);
            ComputerName.Text = pc.Name;
            Connections.Text = pc.Status == 1 ? $"{pc.N_connections}/{pc.Max_connections}" : "";
            switch (pc.Status)
            {
                case 0:
                    ComputerBorder.BorderBrush = CrossGameUtils.GrayBrush;
                    Icon.Kind = PackIconMaterialKind.MonitorOff;
                    break;
                case 1:
                    if (pc.N_connections == 0)
                    {
                        Icon.Kind = PackIconMaterialKind.MonitorClean;
                        ComputerBorder.BorderBrush = CrossGameUtils.BlueBrush;
                    }
                    else if (pc.N_connections == pc.Max_connections)
                    {
                        Icon.Kind = PackIconMaterialKind.MonitorDashboard;
                        ComputerBorder.BorderBrush = CrossGameUtils.RedBrush;
                    }
                    else
                    {
                        Icon.Kind = PackIconMaterialKind.MonitorEye;
                        ComputerBorder.BorderBrush = CrossGameUtils.YellowBrush;
                    }
                    break;
            }
        }
    }
}
