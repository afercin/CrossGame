﻿using System;
using System.Collections.Generic;
using System.Windows;

namespace Cross_Game.Windows
{
    /// <summary>
    /// Lógica de interacción para CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        public CustomMessageBox()
        {
            InitializeComponent();
        }

        private void Window_SourceInitialized(object sender, EventArgs e) => Header.SetWindowHandler(this);

        private void WindowHeader_MenuButtonClick(object sender, ClickEventArgs e) => Close();

        public bool? Show(string text, string caption, MessageBoxButton messageBoxButton)
        {
            Header.HeaderText = caption;

            return ShowDialog();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}
