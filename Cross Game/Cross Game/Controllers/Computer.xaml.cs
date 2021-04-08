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
    /// Lógica de interacción para Computer.xaml
    /// </summary>
    public partial class Computer : UserControl
    {
        private string computerName;
        private string computerIP;
        private string computerStatus;


        public string ComputerName
        {
            get => computerName;
        }

        public string ComputerIP
        {
            get => computerIP;
        }

        public string ComputerStatus
        {
            get => computerIP;
            set => computerStatus = value;
        }

        public Computer()
        {
            InitializeComponent();
        }
    }
}
