using Cross_Game.Connection;
using Cross_Game.Controllers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Cross_Game.Windows
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        public UserData CurrentUser { get; set; }

        private EditComputerParams TransmisionOptions;
        private List<Computer> computerList;
        private OptionButton currentOption;
        private RTDPServer server;
        private Timer connectivity;

        public MainWindow()
        {
            InitializeComponent();

            connectivity = new Timer(600000); // 10 * 60 * 1000
            connectivity.Elapsed += (s, e) => CheckConnection();
            connectivity.Start();

            computerList = new List<Computer>();
            server = null;

            currentOption = Ordenadores;
            Ordenadores.Active = true;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            Header.SetWindowHandler(this);

            UserName.Text = CurrentUser.Name;
            UserNumber.Text = CurrentUser.Number.ToString();

            WaitSlider.SetActions(() =>
            {
                try
                {
                    CurrentUser.SyncLocalMachine();

                    server = new RTDPServer();
                    server.Start(CurrentUser);
                }
                catch (InternetConnectionException ex)
                {
                    LogUtils.AppendLogHeader(LogUtils.ConnectionErrorsLog);
                    LogUtils.AppendLogError(LogUtils.ConnectionErrorsLog, ex.Message);
                    LogUtils.AppendLogFooter(LogUtils.ConnectionErrorsLog);
                }
            }, () =>
            {
                server.Stop();
                server = null;
            });

            CheckConnection();
            TransmisionOptions = new EditComputerParams(CurrentUser);
            TransmisionOptions.Visibility = Visibility.Hidden;
            Content.Children.Add(TransmisionOptions);
        }

        private void OptionButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OptionButton pressedOption = sender as OptionButton;
            if (currentOption != pressedOption)
            {
                currentOption.Active = false;
                ChangeMenuVisibility(currentOption.Name, Visibility.Hidden);
                ChangeMenuVisibility(pressedOption.Name, Visibility.Visible);
                currentOption = pressedOption;
            }
        }

        private void ChangeMenuVisibility(string name, Visibility visibility)
        {
            switch (name)
            {
                case "Ordenadores": MyComputers.Visibility = visibility; break;
                case "Amigos": CheckConnection(); break;
                case "Transmisión": TransmisionOptions.Visibility = visibility;
                    //var display = new UserDisplay();
                    //display.StartTransmission(CurrentUser.localMachine);
                    //try
                    //{
                    //    display.Visibility = Visibility.Visible;
                    //    Hide();
                    //    display.ShowDialog();
                    //    Show();
                    //}
                    //catch
                    //{

                    //}
                    break;
            }
        }

        private void CheckConnection()
        {
            Task.Run(()=>
            {
                List<string> MACs = DBConnection.GetMyComputers(CurrentUser);
                if (MACs != null)
                    Dispatcher.Invoke(() =>
                    {
                        CurrentUser.SyncLocalMachine();

                        LocalIP.Text = CurrentUser.localMachine.LocalIP;
                        PublicIP.Text = CurrentUser.localMachine.PublicIP;

                        if (ConnectionUtils.LastPingResult.RoundtripTime < 200)
                        {
                            ConnectionStatus.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Wifi;
                            ConnectionStatus.Foreground = new SolidColorBrush(Colors.LimeGreen);
                        }
                        else
                        {
                            ConnectionStatus.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.WifiAlert;
                            ConnectionStatus.Foreground = new SolidColorBrush(Colors.Yellow);
                        }

                        Ping.Text = ConnectionUtils.LastPingResult.RoundtripTime + "ms";

                        foreach (string mac in MACs)
                            if (mac != CurrentUser.localMachine.MAC)
                            {
                                var computer = computerList.Find(c => c.pc.MAC == mac);
                                if (computer != null)
                                    computer.UpdateStatus();
                                else
                                {
                                    computer = new Computer(mac);
                                    computer.ComputerClicked += Computer_Clicked;
                                    computerList.Add(computer);

                                    ComputerPanel.Children.Add(computer);
                                }
                            }
                    });
                else
                {
                    ConnectionStatus.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.WifiOff;
                    ConnectionStatus.Foreground = new SolidColorBrush(Colors.Red);
                    Ping.Text = "-";
                }
            });
        }

        private void Computer_Clicked(object sender, EventArgs e)
        {
            ComputerData pc = (sender as ComputerData);
            try
            {
                var display = new UserDisplay();
                display.StartTransmission(pc);
                display.Visibility = Visibility.Visible;
                Hide();
                display.ShowDialog();
            }
            catch { }
            finally
            {
                Show();
            }
        }

        private void WindowHeader_MenuButtonClick(object sender, ClickEventArgs e)
        {
            switch (e.PressedButton)
            {
                case PressedButton.Close: Close(); break;
                case PressedButton.Maximize: WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized; break;
                case PressedButton.Minimize: WindowState = WindowState.Minimized; break;
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e) => Dispose();

        public void Dispose()
        {
            connectivity.Stop();
            connectivity.Dispose();
            server?.Stop();
            DBConnection.LogOut(CurrentUser);
        }
    }
}
