using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cross_Game.Server
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /*
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private Server server;
        private bool closeWindow;

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
        //}

        public MainWindow()
        {
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            System.Windows.Forms.ToolStripMenuItem
                settings = new System.Windows.Forms.ToolStripMenuItem()
                {
                    Name = "Settings",
                    Size = new System.Drawing.Size(208, 22),
                    Text = "Opciones",
                },
                restart = new System.Windows.Forms.ToolStripMenuItem()
                {
                    Name = "Restart",
                    Size = new System.Drawing.Size(208, 22),
                    Text = "Reiniciar"
                },
                close = new System.Windows.Forms.ToolStripMenuItem()
                {
                    Name = "Close",
                    Size = new System.Drawing.Size(208, 22),
                    Text = "Salir"
                };

            settings.Click += (sender, e) => Show();
            restart.Click += (sender, e) => Restart();
            close.Click += (sender, e) => { closeWindow = true; Close(); };

            notifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            notifyIcon.ContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { settings, restart, close });
            notifyIcon.DoubleClick += (sender, e) => Show();

            InitializeComponent();

            try
            {
                string[] data = File.ReadAllLines(Environment.CurrentDirectory + @"\server.conf");
                ConnectionUtils.TcpPort = int.Parse(data[0].Split('=')[1]);
                ConnectionUtils.UdpPort = int.Parse(data[1].Split('=')[1]);
                ConnectionUtils.Delay = 1000 / int.Parse(data[2].Split('=')[1]);
            }
            catch
            {
                ConnectionUtils.TcpPort = 3030;
                ConnectionUtils.UdpPort = 8554;
                ConnectionUtils.Delay = 33;
            }

            server = new Server();
            server.Start();

            notifyIcon.Visible = true;
            notifyIcon.Icon = SystemIcons.Application;
            notifyIcon.ShowBalloonTip(2000, "Todo correcto", "Servidor a la espera de clientes", System.Windows.Forms.ToolTipIcon.Info);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!closeWindow)
                e.Cancel = true;
            else
            {
                notifyIcon.Visible = false;
                server.Stop();
                File.WriteAllLines(Environment.CurrentDirectory + @"\server.conf", new string[]
                {
                    "TcpPort = " + ConnectionUtils.TcpPort,
                    "UdpPort = " + ConnectionUtils.UdpPort,
                    "FPS = " + ConnectionUtils.Delay
                });
            }
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            bool restart = tOptions.Accept();
            if (restart && MessageBox.Show("Para aplicar los cambios es necesario reiniciar. ¿Desea hacerlo ahora?", "Aplicar cambios", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                Restart();
            Hide();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void Button_MouseMove(object sender, RoutedEventArgs e)
        {
            Button currentButton = (Button)sender;
            OverEffect.Visibility = Visibility.Visible;
            OverEffect.Margin = new Thickness(OverEffect.Margin.Left, currentButton.Margin.Top, 0, currentButton.Margin.Bottom);
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
            server.Stop();
            server.Start();
            notifyIcon.ShowBalloonTip(2000, "Reinicio completado", "Servidor a la espera de clientes", System.Windows.Forms.ToolTipIcon.Info);
        }
        */
    }
}
