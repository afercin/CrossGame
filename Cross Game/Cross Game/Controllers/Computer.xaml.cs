using Cross_Game.Connection;
using Cross_Game.Windows;
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
        public event EventHandler ComputerClicked;

        private readonly Brush red = new SolidColorBrush(Color.FromRgb(240, 30, 30));
        private readonly Brush blue = new SolidColorBrush(Color.FromRgb(50, 157, 201));
        private readonly Brush yellow = new SolidColorBrush(Color.FromRgb(190, 170, 40));
        private readonly Brush gray = new SolidColorBrush(Color.FromRgb(140, 140, 140));

        public ComputerData pc;

        public Computer(string mac)
        {
            InitializeComponent();

            UpdateStatus(mac);
        }

        private void Computer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            UpdateStatus();
            if (pc.N_connections < pc.Max_connections && pc.Status != 0)
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
                    ComputerBorder.BorderBrush = gray;
                    Icon.Kind = PackIconMaterialKind.MonitorOff;
                    break;
                case 1:
                    if (pc.N_connections == 0)
                    {
                        Icon.Kind = PackIconMaterialKind.MonitorClean;
                        ComputerBorder.BorderBrush = blue;
                    }
                    else if (pc.N_connections == pc.Max_connections)
                    {
                        Icon.Kind = PackIconMaterialKind.MonitorDashboard;
                        ComputerBorder.BorderBrush = red;
                    }
                    else
                    {
                        Icon.Kind = PackIconMaterialKind.MonitorEye;
                        ComputerBorder.BorderBrush = yellow;
                    }
                    break;
            }
        }
    }
}
