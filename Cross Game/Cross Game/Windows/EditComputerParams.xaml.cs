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
        private Computer current;
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
                current.ComputerName.Text = ComputerName.Text;
                current.pc.Tcp = TCP.Value;
                current.pc.Udp = UDP.Value;
                current.pc.Max_connections = MaxConections.Value;
                DBConnection.UpdateComputerInfo(current);
                DialogResult = true;
            }
            Close();
        }

        public bool? Show(Computer computer)
        {
            current = computer;
            ComputerName.Text = computer.ComputerName.Text.Split(new string[] { " (" }, StringSplitOptions.None)[0];
            TCP.Value = computer.pc.Tcp;
            UDP.Value = computer.pc.Udp;
            MaxConections.Value = computer.pc.Max_connections;
            if (computer.pc.N_connections > 0)
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
            return ComputerName.Text != current.ComputerName.Text.Split(new string[] { " (" }, StringSplitOptions.None)[0] ||
                   TCP.Value != current.pc.Tcp ||
                   UDP.Value != current.pc.Udp ||
                   MaxConections.Value != current.pc.Max_connections;
        }
    }
}
