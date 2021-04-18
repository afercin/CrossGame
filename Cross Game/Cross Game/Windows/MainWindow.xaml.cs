﻿using Cross_Game.Controllers;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Cross_Game.Windows
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OptionButton currentOption;

        public string UserName
        {
            get => DBConnection.User_NickName.Split('#')[0];
        }
        public string UserNumber
        {
            get => DBConnection.User_NickName.Split('#')[1];
        }

        public MainWindow()
        {
            InitializeComponent();
            currentOption = Ordenadores;
            Ordenadores.Active = true;
        }

        private void Window_SourceInitialized(object sender, EventArgs e) => Header.SetWindowHandler(this);

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            new prueba().Show();
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {

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
            switch (currentOption.Name)
            {
                case "Ordenadores": break;
                case "Amigos": break;
                case "Transmisión": break;
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

        private void ActiveServer_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton toogle = sender as ToggleButton;
            if (toogle.IsChecked == true)
            {
                toogle.Background = new SolidColorBrush(Color.FromRgb(153, 153, 153));
                progress.Visibility = Visibility.Visible;
                new Thread(()=>
                {                
                    Thread.Sleep(3000);
                    Dispatcher.Invoke(() => progress.Visibility = Visibility.Hidden);
                    Dispatcher.Invoke(() => toogle.Background = new SolidColorBrush(Color.FromRgb(0, 99, 177)));
                }).Start();
            }
            else
            {
                toogle.Background = new SolidColorBrush(Color.FromRgb(153, 153, 153));
                progress.Visibility = Visibility.Visible;
                new Thread(() =>
                {
                    Thread.Sleep(3000);
                    Dispatcher.Invoke(() => progress.Visibility = Visibility.Hidden);
                    Dispatcher.Invoke(() => toogle.Background = new SolidColorBrush(Color.FromRgb(0, 99, 177)));
                }).Start();
            }
        }
    }
}
