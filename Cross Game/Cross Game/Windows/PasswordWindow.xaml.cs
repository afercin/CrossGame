using MahApps.Metro.IconPacks;
using RTDP;
using System;
using System.IO;
using System.Windows;

namespace Cross_Game.Windows
{
    /// <summary>
    /// Lógica de interacción para CustomMessageBox.xaml
    /// </summary>
    public partial class PasswordWindow : Window
    {
        private static string PasswordPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cross Game", "autologin.info");
        public PasswordWindow()
        {
            InitializeComponent();
        }

        private void Window_SourceInitialized(object sender, EventArgs e) => Header.SetWindowHandler(this);

        public string GetPassword(string mac, string serverName, bool error = false)
        {
            if (error)
            {
                Alert.Kind = PackIconMaterialKind.AlertCircleOutline;
                Alert.Foreground = CrossGameUtils.RedBrush;
            }
            else
            {
                Alert.Kind = PackIconMaterialKind.AlertOutline;
                Alert.Foreground = CrossGameUtils.YellowBrush;
            }

            if (serverName == string.Empty)
                Message.Text = "Antes de continuar es necesario que establezca una contraseña para que otros equipos se puedan conectar con su equipo.";
            else
                Message.Text = $"Antes de continuar es necesario que introduzca la contraseña que ha sido establecida en el equipo \"{serverName}\".";

            ShowDialog();
            if (DialogResult == true)
            {
                if (RememberMe.IsChecked == true)
                    try
                    {
                        byte[] encryptPassword = Crypto.GetBytes(Crypto.CreateSHA256(CrossGameUtils.GetWindowDiskSN(LogUtils.LoginLog)));
                        Array.Resize(ref encryptPassword, 32);

                        Crypto.WriteData(Path.Combine(LogUtils.CrossGameFolder, $"{mac}.password"), encryptPassword, new string[] { Password.Password });
                    }
                    catch (Exception ex)
                    {
                        LogUtils.AppendLogError(LogUtils.PasswordErrorsLog, "Ha ocurrido un error al generar el fichero: " + ex);
                    }
                return Password.Password;
            }
            else
                return string.Empty;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Header_MenuButtonClick(object sender, ClickEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
