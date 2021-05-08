using System;
using System.Windows;
using System.Windows.Controls;
using Cross_Game.Controllers;

namespace Cross_Game.Windows
{
    /// <summary>
    /// Lógica de interacción para EditComputerParams.xaml
    /// </summary>
    public partial class EditComputerParams : UserControl
    {
        private readonly ComputerData localMachine;
        public EditComputerParams(ComputerData computer)
        {
            InitializeComponent();
            localMachine = computer;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (Edited())
            {
                localMachine.Name = ComputerName.Text;
                localMachine.Tcp = TCP.Value;
                localMachine.Udp = UDP.Value;
                localMachine.Max_connections = MaxConections.Value;
                localMachine.FPS = FPS.Value;
                DBConnection.UpdateComputerInfo(localMachine);
            }
        }

        private bool Edited()
        {
            return ComputerName.Text != localMachine.Name ||
                   TCP.Value != localMachine.Tcp || UDP.Value != localMachine.Udp ||
                   MaxConections.Value != localMachine.Max_connections || FPS.Value != localMachine.FPS;
        }

        private void EditComputer_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                ComputerName.Text = localMachine.Name;
                TCP.Value = localMachine.Tcp;
                UDP.Value = localMachine.Udp;
                MaxConections.Value = localMachine.Max_connections;
                FPS.Value = localMachine.FPS;
                Alert.Visibility = localMachine.Status != 0 ? Visibility.Visible : Visibility.Hidden;
            }
        }
    }
}
