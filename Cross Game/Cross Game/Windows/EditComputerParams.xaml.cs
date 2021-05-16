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
        private bool isAlerted;
        public bool IsAlerted
        {
            set
            {
                isAlerted = value;
                Alert.Visibility = value ? Visibility.Visible : Visibility.Hidden;
                TCP.IsEnabled = !value;
                UDP.IsEnabled = !value;
                MaxConections.IsEnabled = !value;
                FPS.IsEnabled = !value;
            }
        }

        private readonly UserData currentUser;
        public EditComputerParams(UserData user)
        {
            InitializeComponent();
            currentUser = user;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (Edited())
            {
                currentUser.localMachine.Name = ComputerName.Text;
                currentUser.localMachine.Tcp = TCP.Value;
                currentUser.localMachine.Udp = UDP.Value;
                currentUser.localMachine.Max_connections = MaxConections.Value;
                currentUser.localMachine.FPS = FPS.Value;
                DBConnection.UpdateTransmissionConf(currentUser.localMachine);
            }
        }

        private bool Edited()
        {
            return ComputerName.Text != currentUser.localMachine.Name ||
                   TCP.Value != currentUser.localMachine.Tcp || UDP.Value != currentUser.localMachine.Udp ||
                   MaxConections.Value != currentUser.localMachine.Max_connections || FPS.Value != currentUser.localMachine.FPS;
        }

        private void EditComputer_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                ComputerName.Text = currentUser.localMachine.Name;
                TCP.Value = currentUser.localMachine.Tcp;
                UDP.Value = currentUser.localMachine.Udp;
                MaxConections.Value = currentUser.localMachine.Max_connections;
                FPS.Value = currentUser.localMachine.FPS;
                IsAlerted = currentUser.localMachine.Status != 0;
            }
        }
    }
}
