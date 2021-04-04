using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Cross_Game
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool closeWindow;

        public MainWindow()
        {
            InitializeComponent();
            LoginRegister lr = new LoginRegister();
            lr.Show();
        }

        //static string GetIPAddress()
        //{
        //    String address = "";
        //    WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
        //    using (WebResponse response = request.GetResponse())
        //    using (StreamReader stream = new StreamReader(response.GetResponseStream()))
        //    {
        //        address = stream.ReadToEnd();
        //    }

        //    int first = address.IndexOf("Address: ") + 9;
        //    int last = address.LastIndexOf("</body>");
        //    address = address.Substring(first, last - first);

        //    return address;
        //

        private void Window_Closing(object sender, CancelEventArgs e)
        {

        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void Button_MouseMove(object sender, RoutedEventArgs e)
        {

        }

        private void Button_MouseLeave(object sender, RoutedEventArgs e) => OverEffect.Visibility = Visibility.Hidden;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button currentButton = (Button)sender;
            SelectEffect.Margin = new Thickness(SelectEffect.Margin.Left, currentButton.Margin.Top, 0, currentButton.Margin.Bottom);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Hide();

        private void Close_Click(object sender, RoutedEventArgs e) => Hide();

        private void Restart()
        {

        }
    }
}
