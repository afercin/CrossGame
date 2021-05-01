using System;
using System.Windows;
using Cross_Game.Controllers;

namespace Cross_Game.Windows
{
    /// <summary>
    /// Lógica de interacción para EditComputerParams.xaml
    /// </summary>
    public partial class EditComputerParams : Window
    {
        private ComputerData current;
        public EditComputerParams()
        {
            InitializeComponent();
        }

        private void Window_SourceInitialized(object sender, EventArgs e) => Header.SetWindowHandler(this);

        private void WindowHeader_MenuButtonClick(object sender, ClickEventArgs e) => Close();

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (Edited())
            {
                current.Name = ComputerName.Text;
                current.Tcp = TCP.Value;
                current.Udp = UDP.Value;
                current.Max_connections = MaxConections.Value;
                current.FPS = FPS.Value;
                DBConnection.UpdateComputerInfo(current);
                DialogResult = true;
            }
            Close();
        }

        public bool? Show(ComputerData computer)
        {
            current = computer;
            ComputerName.Text = computer.Name;
            TCP.Value = computer.Tcp;
            UDP.Value = computer.Udp;
            MaxConections.Value = computer.Max_connections;
            FPS.Value = computer.FPS;
            if (computer.N_connections > 0)
            {
                TCP.IsEnabled = false;
                UDP.IsEnabled = false;
            }
            return ShowDialog();
        }

        private void EditComputer_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = Edited() &&  MessageBox.Show("Se perderá la nueva configuración. ¿Desea continuar?", "Cancelar edición", 
                                                    MessageBoxButton.YesNo, MessageBoxImage.Exclamation) != MessageBoxResult.Yes;
        }

        private bool Edited()
        {
            return ComputerName.Text != current.Name ||
                   TCP.Value != current.Tcp || UDP.Value != current.Udp ||
                   MaxConections.Value != current.Max_connections || FPS.Value != current.FPS;
        }
    }
}
