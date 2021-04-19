﻿using Cross_Game.Windows;
using MahApps.Metro.IconPacks;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cross_Game.Controllers
{
    /// <summary>
    /// Lógica de interacción para Computer.xaml
    /// </summary>
    public partial class Computer : UserControl
    {
        private readonly Brush red = new SolidColorBrush(Color.FromRgb(240, 30, 30));
        private readonly Brush blue = new SolidColorBrush(Color.FromRgb(50, 157, 201));
        private readonly Brush yellow = new SolidColorBrush(Color.FromRgb(240, 30, 30));
        private readonly Brush gray = new SolidColorBrush(Color.FromRgb(224, 224, 224));

        public readonly PC pc;
        private bool localMachine;
        private bool editArea;
        private PackIconMaterialKind currentIcon;

        public Computer(string _MAC = "")
        {

            InitializeComponent();
            pc = new PC();
            pc.MAC = _MAC;
            localMachine = string.IsNullOrEmpty(pc.MAC);

            UpdateStatus();
        }

        private void Computer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!editArea)
            {
                UpdateStatus();
                if (pc.N_connections < pc.Max_connections)
                    MessageBox.Show("conectandose no más");
            }
            else if (new EditComputerParams().Show(this) == true)
            {
                DBConnection.UpdateComputerInfo(this);
                ComputerName.Text += (localMachine ? " (local)" : "");
            }
            Icon.Kind = currentIcon;
            editArea = false;
        }

        public void UpdateStatus()
        {
            DBConnection.GetComputerData(pc);
            ComputerName.Text = pc.Name;

            if (pc.Status != -1)
            {
                ComputerName.Text += (localMachine ? " (local)" : "");

                Connections.Text = pc.Status == 1 ? $"{pc.N_connections}/{pc.Max_connections}" : "";
                if (pc.N_connections == 0)
                {
                    switch (pc.Status)
                    {
                        case 0:
                            ComputerBorder.BorderBrush = gray;
                            Icon.Kind = PackIconMaterialKind.MonitorOff;
                            break;
                        case 1:
                            ComputerBorder.BorderBrush = blue;
                            Icon.Kind = PackIconMaterialKind.MonitorClean;
                            break;
                    }
                }
                else if (pc.N_connections == -1 || pc.N_connections == pc.Max_connections)
                {
                    Icon.Kind = PackIconMaterialKind.MonitorDashboard;
                    ComputerBorder.BorderBrush = red;
                }
                else
                {
                    Icon.Kind = PackIconMaterialKind.MonitorEye;
                    ComputerBorder.BorderBrush = yellow;
                }

                currentIcon = Icon.Kind;
            }
            else
                throw new Exception("Invalid computer status.");
        }

        private void ComputerName_MouseEnter(object sender, MouseEventArgs e)
        {
            Icon.Kind = PackIconMaterialKind.MonitorEdit;
            editArea = true;
        }

        private void ComputerName_MouseLeave(object sender, MouseEventArgs e)
        {
            Icon.Kind = currentIcon;
            editArea = false;
        }
    }
}
