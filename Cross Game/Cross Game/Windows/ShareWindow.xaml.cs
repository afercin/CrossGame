using RTDP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Cross_Game.Windows
{
    /// <summary>
    /// Lógica de interacción para ShareWindow.xaml
    /// </summary>
    public partial class ShareWindow : Window
    {
        private bool accepted;
        private List<SharedComputer> computers;

        public ShareWindow()
        {
            InitializeComponent();
            computers = new List<SharedComputer>();
            foreach (string mac in DBConnection.GetUserComputers())
            {
                var c = DBConnection.GetComputerData(mac);
                Computers.ItemsSource = computers;
                computers.Add(new SharedComputer()
                {
                    MAC = c.MAC,
                    Name = c.Name,
                    LocalIP = c.LocalIP,
                    PublicIP = c.PublicIP,
                    AccessAllowed = false
                });
            }
        }

        private void Window_SourceInitialized(object sender, EventArgs e) => Header.SetWindowHandler(this);

        public void Show(string friendName, int friendNumber)
        {
            foreach(string mac in DBConnection.GetComputersSharedWithFriend(friendName, friendNumber))
            {
                var computer = computers.Find(c => c.MAC == mac);
                if (computer != null)
                    computer.AccessAllowed = true;
            }

            var computers_old = computers.Select(item => (SharedComputer)item.Clone()).ToList();

            ShowDialog();

            if (accepted)
                foreach (SharedComputer computer in computers)
                    if (computer.AccessAllowed != computers_old.Find(c => c.MAC == computer.MAC).AccessAllowed)
                        DBConnection.ManageComputerAccess(friendName, friendNumber, computer.MAC, computer.AccessAllowed);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            accepted = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            accepted = false;
            Close();
        }

        private void Header_MenuButtonClick(object sender, ClickEventArgs e)
        {
            accepted = false;
            Close();
        }

        private class SharedComputer : ICloneable
        {
            private string mac;
            private string name;
            private string localIP;
            private string publicIP;
            private bool accessAllowed;
            public string Name { get => name; set => name = value; }
            public string LocalIP { get => localIP; set => localIP = value; }
            public string PublicIP { get => publicIP; set => publicIP = value; }
            public bool AccessAllowed { get => accessAllowed; set => accessAllowed = value; }
            public string MAC { get => mac; set => mac = value; }

            public object Clone()
            {
                return new SharedComputer()
                {
                    mac = MAC,
                    name = Name,
                    localIP = LocalIP,
                    publicIP = PublicIP,
                    accessAllowed = AccessAllowed
                };
            }
        }
    }
}
