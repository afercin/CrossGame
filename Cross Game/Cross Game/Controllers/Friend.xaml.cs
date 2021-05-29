using MahApps.Metro.IconPacks;
using RTDP;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cross_Game.Controllers
{
    /// <summary>
    /// Lógica de interacción para Computer.xaml
    /// </summary>
    public partial class Friend : UserControl
    {
        public event EventHandler FriendClicked;
        
        public string GameTag { get => FriendName.Text; }
        public string name;
        public string number;

        public Friend(string gameTag)
        {
            InitializeComponent();
            FriendName.Text = gameTag;
            name = gameTag.Split('#')[0];
            number = gameTag.Split('#')[1];
            UpdateStatus();
        }

        private void Friend_MouseDown(object sender, MouseButtonEventArgs e)
        {
            UpdateStatus();
            FriendClicked.Invoke(this, EventArgs.Empty);
        }

        public void UpdateStatus()
        {
            switch (DBConnection.GetUserStatus(name, number))
            {
                case 0:
                    FriendBorder.BorderBrush = CrossGameUtils.GrayBrush;
                    Icon.Kind = PackIconMaterialKind.AccountOff;
                    break;
                case 1:
                    FriendBorder.BorderBrush = CrossGameUtils.BlueBrush;
                    Icon.Kind = PackIconMaterialKind.Account;
                    break;
                case 2:
                    FriendBorder.BorderBrush = CrossGameUtils.YellowBrush;
                    Icon.Kind = PackIconMaterialKind.Account;
                    break;
            }
        }
    }
}
