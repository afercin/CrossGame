using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Cross_Game
{
    /// <summary>
    /// Lógica de interacción para prueba.xaml
    /// </summary>
    public partial class prueba : Window
    {
        private bool blocked;
        public prueba()
        {
            InitializeComponent();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void LayoutRoot_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!blocked)
            {
                blocked = true;
                Check.IsEnabled = false;
                Check.IsChecked = !Check.IsChecked;
                new Thread(() =>
                {
                    Thread.Sleep(3000);
                    Dispatcher.Invoke(() => Check.IsEnabled = true);
                    blocked = false;
                }).Start();
            }
        }
    }
}
