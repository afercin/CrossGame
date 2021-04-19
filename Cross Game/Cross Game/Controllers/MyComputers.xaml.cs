using System;
using System.Windows;
using System.Windows.Controls;

namespace Cross_Game.Controllers
{
    /// <summary>
    /// Lógica de interacción para MyComputers.xaml
    /// </summary>
    public partial class MyComputers : UserControl
    {
        public MyComputers()
        {
            InitializeComponent();
            foreach (string mac in DBConnection.GetMyComputers())
                if (mac != DBConnection.CurrentUser.localMachine.MAC)
                {
                    try
                    {
                        ComputerPanel.Children.Add(new Computer(mac));
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }
        }
    }
}
