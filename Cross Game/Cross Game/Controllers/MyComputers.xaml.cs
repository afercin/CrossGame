using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Lógica de interacción para MyComputers.xaml
    /// </summary>
    public partial class MyComputers : UserControl
    {
        private Computer currentComputer;
        public MyComputers()
        {
            InitializeComponent();
            currentComputer = new Computer()
            {
                Margin = new Thickness(5)
            };
            ComputerPanel.Children.Add(currentComputer);
            foreach (string mac in DBConnection.GetMyComputers())
                if (mac != currentComputer.ComputerMAC)
                    ComputerPanel.Children.Add(new Computer(mac)
                    {
                        Margin = new Thickness(5)
                    });
        }

        public void LogOut() => DBConnection.LogOut(currentComputer.ComputerMAC);
    }
}
