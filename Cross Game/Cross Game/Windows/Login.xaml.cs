using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cross_Game.Windows
{
    /// <summary>
    /// Ventana encargada de solicitar y verificar las credenciales del cliente.
    /// </summary>
    public partial class Login : Window
    {
        private readonly string AutoLoginPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cross Game", "autologin.info");
        private readonly Brush White = new SolidColorBrush(Colors.White);

        private const string watermakEmail = "email@example.com";
        private const string watermakPassword = "email@example.c";
        private bool emailWatermark;
        private bool passwordWatermark;

        public Login()
        {
            LogUtils.CleanLogs();

            InitializeComponent();
            emailWatermark = true;
            passwordWatermark = true;
            Error.Visibility = Visibility.Hidden;

            LogUtils.AppendLogHeader(LogUtils.LoginLog);
            
            try
            {
                string email, md5_pass;
                using (BinaryReader br = new BinaryReader(File.Open(AutoLoginPath, FileMode.Open)))
                {
                    email = br.ReadString();
                    md5_pass = br.ReadString();
                }

                LogUtils.AppendLogOk(LogUtils.LoginLog, "Se ha detectado el fichero de autologin, comprobando las credenciales al usuario...");

                Email.Text = email;
                Password.Password = "";
                CheckLogin(email, md5_pass, true);
            }
            catch (IOException)
            {
                LogUtils.AppendLogWarn(LogUtils.LoginLog, "No existe el fichero de autologin, se le solicitarán las credenciales al usuario.");
            }

        }

        private void Login_OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e) => Process.Start("http://crossgame.sytes.net/myaccount/register");

        private void Button_Click(object sender, RoutedEventArgs e) => CheckLogin(Email.Text, Password.Password);

        private void Field_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                CheckLogin(Email.Text, Password.Password);
        }

        private void Field_GotFocus(object sender, RoutedEventArgs e)
        {
            Control field = sender as Control;
            switch (field.Name)
            {
                case "Email": if (emailWatermark) Email.Text = ""; break;
                case "Password": if (passwordWatermark) Password.Password = ""; break;
            }
        }

        private void Field_LostFocus(object sender, RoutedEventArgs e)
        {
            Control field = sender as Control;
            switch (field.Name)
            {
                case "Email": if (emailWatermark) Email.Text = watermakEmail; break;
                case "Password": if (passwordWatermark) Password.Password = watermakPassword; break;
            }
        }

        private void Email_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Email.Text != string.Empty && Email.Text != watermakEmail && emailWatermark)
                emailWatermark = false;
            if (Email.Text == string.Empty && !emailWatermark)
                emailWatermark = true;
            if (Error != null && Error.Visibility == Visibility.Visible && !emailWatermark)
            {
                Error.Visibility = Visibility.Hidden;
                Email.Foreground = White;
                Email.Foreground = White;
            }
        }

        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (Password.Password != string.Empty && Password.Password != watermakPassword && passwordWatermark)
                passwordWatermark = false;
            if (Password.Password == string.Empty && !passwordWatermark)
                passwordWatermark = true;
            if (Error != null && Error.Visibility == Visibility.Visible && !passwordWatermark)
            {
                Error.Visibility = Visibility.Hidden;
                Email.Foreground = White;
                Email.Foreground = White;
            }
        }

        private void Error_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((sender as TextBlock).Visibility == Visibility.Visible)
            {
                Email.Foreground = Error.Foreground;
                Password.Foreground = Error.Foreground;
                CheckText.Foreground = Error.Foreground;
            }
            else
            {
                Email.Foreground = White;
                Password.Foreground = White;
                CheckText.Foreground = White;
            }
        }
        /// <summary>
        /// Comprueba las credenciales de un cliente a partir de su email y contraseña, se puede especificar si la contraseña ya está cifrada (autologin).
        /// </summary>
        private void CheckLogin(string email, string password, bool md5 = false)
        {
            if (email != watermakEmail && password != watermakPassword)
            {
                LogUtils.AppendLogText(LogUtils.LoginLog, "Comprobando las credenciales introducidas...");

                UserData currentUser = DBConnection.CheckLogin(email, password, md5);

                if (currentUser == null)
                {
                    LogUtils.AppendLogError(LogUtils.LoginLog, "Se ha producido un error al conectar con la base de datos.");
                    Error.Text = "Error de conexión.";
                    Error.Visibility = Visibility.Visible;
                }
                else if (currentUser.ID == 0)
                {
                    LogUtils.AppendLogWarn(LogUtils.LoginLog, "Las credenciales introducidas no concuerdan con las de ningún usuario.");
                    Error.Text = "Email o contraseña incorrectos.";
                    Error.Visibility = Visibility.Visible;
                }
                else
                {
                    LogUtils.AppendLogOk(LogUtils.LoginLog, "Las credenciales introducidas son correctas.");
                    if (RememberMe.IsChecked == true)
                    {
                        LogUtils.AppendLogText(LogUtils.LoginLog, "Remember me estaba marcado por lo que se procede guardar las credenciales en el fichero de autologin.");

                        if (!Directory.Exists(Path.GetDirectoryName(AutoLoginPath)))
                            Directory.CreateDirectory(Path.GetDirectoryName(AutoLoginPath));
                        using (BinaryWriter bw = new BinaryWriter(File.Open(AutoLoginPath, FileMode.Create)))
                        {
                            bw.Write(email);
                            bw.Write(md5 ? password : DBConnection.CreateMD5(password));
                        }
                    }

                    LogUtils.AppendLogText(LogUtils.LoginLog, "Iniciando instancia de la ventana principal...");

                    currentUser.SyncLocalMachine();
                    var mainWindow = new MainWindow
                    {
                        CurrentUser = currentUser
                    };
                    mainWindow.Show();

                    LogUtils.AppendLogText(LogUtils.LoginLog, "Cerrando ventana para logearse.");

                    Close();
                }
            }
                
        }

        private void Window_Closed(object sender, EventArgs e) => LogUtils.AppendLogFooter(LogUtils.LoginLog);
    }
}
