using Cross_Game.Controllers;
using RTDP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public partial class MainWindow : Window
    {
        public UserData CurrentUser { get; set; }

        private EditComputerParams TransmisionOptions;
        private List<Computer> computerList;
        private List<Friend> friendList;
        private OptionButton currentOption;
        private RTDPServer server;
        private Timer connectivity;

        public MainWindow()
        {
            InitializeComponent();

            connectivity = new Timer(300000); // 5 * 60 * 1000
            connectivity.Elapsed += (s, e) => SyncComputers();
            connectivity.Start();
            
            computerList = new List<Computer>();
            friendList = new List<Friend>();

            currentOption = Ordenadores;
            Ordenadores.Active = true;
        }

        #region Server On/Off

        private void ServerStart()
        {
            string password = string.Empty, passFile = Path.Combine(LogUtils.CrossGameFolder, $"{CurrentUser.localMachine.MAC}.password");
            bool error = false;
            try
            {
                CurrentUser.SyncLocalMachine(1);
                try
                {
                    LogUtils.AppendLogHeader(LogUtils.ServerConnectionLog);

                    byte[] decryptPassword = Crypto.GetBytes(Crypto.CreateSHA256(CrossGameUtils.GetWindowDiskSN(LogUtils.LoginLog)));
                    Array.Resize(ref decryptPassword, 32);
                    LogUtils.AppendLogHeader(LogUtils.ServerConnectionLog);

                    password = Crypto.ReadData(passFile, decryptPassword)[0];
                    error = false;

                    LogUtils.AppendLogOk(LogUtils.ServerConnectionLog, $"Se ha detectado el fichero con la contraseña de del equipo actual. Iniciando proceso de conexión...");
                }
                catch (IOException)
                {
                    LogUtils.AppendLogWarn(LogUtils.ServerConnectionLog, "No existe el fichero con la contraseña de del equipo actual, se le solicitará al usuario.");
                    password = string.Empty;
                    error = false;
                }
                catch (Exception)
                {
                    LogUtils.AppendLogWarn(LogUtils.ServerConnectionLog, "Error al leer el fichero con la contraseña de del equipo actual, se le solicitará al usuario.");
                    File.Delete(passFile);
                    password = string.Empty;
                    error = true;
                }

                if (password == string.Empty)
                    Dispatcher.Invoke(() => password = new PasswordWindow().GetPassword(CurrentUser.localMachine.MAC, string.Empty, error));

                if (password != string.Empty)
                {
                    server = new RTDPServer(password);
                    server.ConnectedClientsChanged += (s, a) => DBConnection.UpdateComputerStatus(CurrentUser.localMachine);
                    server.GotClientCredentials += (s, a) =>
                    {
                        a.UserPriority = 0;
                        if (DBConnection.CheckLogin(a.email, a.password, false).Number != 0) // El usuario existe
                        {
                            var clientComputer = DBConnection.GetComputerData(a.mac);
                            var computerList = DBConnection.GetUserComputers(a.email, a.password);
                            if (computerList.Contains(a.mac))   // El usuario es la mismo que el del servidor.
                                a.UserPriority = 2;
                            else
                            {
                                computerList = DBConnection.GetSharedComputers(a.email, a.password);
                                if (computerList.Contains(a.mac))
                                    a.UserPriority = 1;
                            }
                            if (a.localIP != clientComputer.LocalIP || a.publicIP != clientComputer.PublicIP)
                                a.UserPriority = 0;
                        }
                    };

                    server.Start(ref CurrentUser.localMachine);
                    if (TransmisionOptions.Visibility == Visibility.Visible)
                        Dispatcher.Invoke(() => TransmisionOptions.IsAlerted = true);

                    WaitSlider.Done = true;
                }
                else
                {
                    WaitSlider.Done = false;
                    Task.Run(() =>
                    {
                        System.Threading.Thread.Sleep(50);
                        Dispatcher.Invoke(() => WaitSlider.Slider_MouseDown(null, null));
                    });
                }
            }
            catch (InternetConnectionException ex)
            {
                LogUtils.AppendLogError(LogUtils.ServerConnectionLog, ex.Message);
                LogUtils.AppendLogFooter(LogUtils.ServerConnectionLog);
            }
        }

        private void ServerStop()
        {
            server?.Stop();
            if (TransmisionOptions.Visibility == Visibility.Visible)
                Dispatcher.Invoke(() => TransmisionOptions.IsAlerted = false);

            CurrentUser.SyncLocalMachine(0);
        }

        #endregion

        #region Sync data

        private void SyncComputers()
        {
            Task.Run(()=>
            {
                CurrentUser.SyncLocalMachine();
                List<string> MACs = DBConnection.GetUserComputers();
                if (MACs != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        LocalIP.Text = CurrentUser.localMachine.LocalIP;
                        PublicIP.Text = CurrentUser.localMachine.PublicIP;

                        if (CrossGameUtils.LastPingResult.RoundtripTime < 200)
                        {
                            ConnectionStatus.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Wifi;
                            ConnectionStatus.Foreground = CrossGameUtils.GreenBrush;
                        }
                        else
                        {
                            ConnectionStatus.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.WifiAlert;
                            ConnectionStatus.Foreground = CrossGameUtils.YellowBrush;
                        }

                        Ping.Text = CrossGameUtils.LastPingResult.RoundtripTime + "ms";
                    });

                    Computer[] deletedComputers = computerList.ToArray();

                    Dispatcher.Invoke(() =>
                    {
                        foreach (string mac in MACs)
                            if (mac != CurrentUser.localMachine.MAC)
                            {
                                var computer = computerList.Find(c => c.pc.MAC == mac);

                                if (computer != null)
                                {
                                    computer.UpdateStatus();
                                    deletedComputers = deletedComputers.Where(c => c.pc.MAC != computer.pc.MAC).ToArray();
                                }
                                else
                                {
                                    computer = new Computer(mac);
                                    computer.ComputerClicked += Computer_Clicked;
                                    computerList.Add(computer);

                                    ComputerPanel.Children.Add(computer);
                                }
                            }

                        foreach (Computer c in deletedComputers)
                        {
                            computerList.Remove(c);
                            ComputerPanel.Children.Remove(c);
                        }
                    });
                }                    
                else
                {
                    ConnectionStatus.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.WifiOff;
                    ConnectionStatus.Foreground = new SolidColorBrush(Colors.Red);
                    Ping.Text = "-";
                }
                Dispatcher.Invoke(() => RefreshText.Text = DateTime.Now.ToLongTimeString());
            });
        }

        private void SyncFriends()
        {
            Task.Run(() =>
            {
                var friends = DBConnection.GetFriends();
                if (friends != null)
                {
                    Friend[] deletedFriends = friendList.ToArray();
                    Dispatcher.Invoke(() =>
                    {
                        foreach (string gameTag in friends)
                        {
                            var friend = friendList.Find(f => f.GameTag == gameTag);
                            if (friend != null)
                            {
                                friend.UpdateStatus();
                                deletedFriends = deletedFriends.Where(f => f.GameTag != gameTag).ToArray();
                            }
                            else
                            {
                                friend = new Friend(gameTag);
                                friend.FriendClicked += Friend_FriendClicked;
                                friendList.Add(friend);

                                FriendsPanel.Children.Add(friend);
                            }
                        }

                        foreach (Friend f in deletedFriends)
                        {
                            friendList.Remove(f);
                            FriendsPanel.Children.Remove(f);
                        }
                        RefreshText.Text = DateTime.Now.ToLongTimeString();
                    });
                }             
            });
        }

        #endregion

        #region Window events

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            Header.SetWindowHandler(this);

            UserName.Text = CurrentUser.Name;
            UserNumber.Text = CurrentUser.Number.ToString();

            WaitSlider.SetActions(ServerStart, ServerStop);

            SyncComputers();
            TransmisionOptions = new EditComputerParams(CurrentUser);
            TransmisionOptions.Visibility = Visibility.Hidden;
            Panels.Children.Add(TransmisionOptions);
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
                case "Ordenadores":
                    MyComputers.Visibility = visibility;
                    RefreshButton.Text = "Actualizar ordenadores";
                    ComputerBorder.Visibility = visibility;
                    if (visibility == Visibility.Visible)
                        SyncComputers();
                    break;
                case "Amigos":
                    MyFriends.Visibility = visibility;
                    RefreshButton.Text = "Actualizar amigos";
                    ComputerBorder.Visibility = visibility;
                    if (visibility == Visibility.Visible)
                        SyncFriends();
                    break;
                case "Transmisión":
                    TransmisionOptions.Visibility = visibility;
                    break;
            }
        }

        private void Computer_Clicked(object sender, EventArgs e)
        {
            ComputerData pc = sender as ComputerData;
            if (pc.Status != -1)
            {
                try
                {
                    var display = new UserDisplay();
                    display.StartTransmission(ref pc);
                    display.Visibility = Visibility.Visible;
                    Hide();
                    CurrentUser.Status = 2;
                    DBConnection.UpdateUserStatus(CurrentUser);
                    display.ShowDialog();
                }
                catch { }
                finally
                {
                    CurrentUser.Status = 1;
                    DBConnection.UpdateUserStatus(CurrentUser);
                    Show();
                }
            }
            else
            {
                Computer removedComputer = computerList.Find(c => c.pc.MAC == pc.MAC);
                computerList.Remove(removedComputer);
                ComputerPanel.Children.Remove(removedComputer);
            }
        }

        private void Friend_FriendClicked(object sender, EventArgs e)
        {
            Friend f = sender as Friend;
            new ShareWindow().Show(f.name, f.number);
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

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            connectivity.Stop();
            connectivity.Dispose();
            server?.Stop();
            DBConnection.LogOut(CurrentUser);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (RefreshButton.Text == "Actualizar ordenadores")
                SyncComputers();
            else
                SyncFriends();
        }

        #endregion
    }
}
