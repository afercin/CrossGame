using Cross_Game.Connection;
using Cross_Game.Controllers;
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
        private RTDPController server;

        public MainWindow()
        {
            InitializeComponent();
            currentOption = Ordenadores;
            Ordenadores.Active = true;

            WaitSlider.SetActions(() => server = new RTDPController(3030, 3031, 1, 30), () => server.Dispose());
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
                case "Transmisión": new RTDPController(3030,3031,"127.0.0.1") ; break;
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
            DBConnection.LogOut();
        }
    }
}
