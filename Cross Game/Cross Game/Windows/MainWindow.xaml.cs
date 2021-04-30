using Cross_Game.Connection;
using Cross_Game.Controllers;
using Cross_Game.DataManipulation;
using System;
using System.Windows;
using System.Windows.Input;

namespace Cross_Game.Windows
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string UserName { get => DBConnection.CurrentUser.Name; }
        public string UserNumber { get => DBConnection.CurrentUser.Number.ToString(); }

        private OptionButton currentOption;
        private RTDPServer server;

        public MainWindow()
        {
            Screen.CaptureScreen();
            InitializeComponent();
            currentOption = Ordenadores;
            Ordenadores.Active = true;

            WaitSlider.SetActions(() => 
            {
                server = new RTDPServer(3030, 3031);
                server.MaxConnections = 1;
                server.TimeRate = 1000 / 45;
                server.Start();
            }, () => server.Stop());
        }

        private void Window_SourceInitialized(object sender, EventArgs e) => Header.SetWindowHandler(this);

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //var w = new WaitingWindow();
            //w.Show();
            //w.SetText("Reconectando con el servidor...");
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
                case "Ordenadores": break;
                case "Amigos": new WaitingWindow().Show(); break;
                case "Transmisión":
                    var display = new UserDisplay();
                    display.StartTransmission(3030, 3031, "127.0.0.1");
                    break;
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

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            server?.Stop();
            DBConnection.LogOut();
        }
    }
}
