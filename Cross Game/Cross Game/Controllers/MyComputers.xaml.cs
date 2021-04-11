﻿using System.Windows;
using System.Windows.Controls;

namespace Cross_Game.Controllers
{
    /// <summary>
    /// Lógica de interacción para MyComputers.xaml
    /// </summary>
    public partial class MyComputers : UserControl
    {
        private Computer currentComputer;
        public MyComputers()
        {
            InitializeComponent();
            currentComputer = new Computer()
            {
                Margin = new Thickness(5)
            };
            ComputerPanel.Children.Add(currentComputer);
            foreach (string mac in DBConnection.GetMyComputers())
                if (mac != currentComputer.ComputerMAC)
                    ComputerPanel.Children.Add(new Computer(mac)
                    {
                        Margin = new Thickness(5)
                    });
        }

        public void LogOut() => DBConnection.LogOut(currentComputer.ComputerMAC);
    }
}
