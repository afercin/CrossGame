using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cross_Game
{
    /// <summary>
    /// Lógica de interacción para LoginRegister.xaml
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
            InitializeComponent();

            emailWatermark = true;
            passwordWatermark = true;
            Error.Visibility = Visibility.Hidden;

            try
            {
                string email, md5_pass;
                using (BinaryReader br = new BinaryReader(File.Open(AutoLoginPath, FileMode.Open)))
                {
                    email = br.ReadString();
                    md5_pass = br.ReadString();
                }
                Email.Text = email;
                Password.Password = "";
                CheckLogin(email, md5_pass, true);
            }
            catch
            {
                // No existe el fichero;
            }

        }

        private void Login_OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e) => Process.Start("http://localhost/codeigniter3/register");

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

        private void CheckLogin(string email, string password, bool md5 = false)
        {
            if (email != watermakEmail && password != watermakPassword)
                switch (DBConnection.CheckLogin(email, password, md5))
                {
                    case -1:
                        Error.Text = "Error de conexión.";
                        Error.Visibility = Visibility.Visible;
                        break;
                    case 0:
                        Error.Text = "Email o contraseña incorrectos.";
                        Error.Visibility = Visibility.Visible;
                        break;
                    case 1:
                        if (DBConnection.User_ID != -1)
                        {
                            if (RememberMe.IsChecked == true)
                            {
                                if (!Directory.Exists(Path.GetDirectoryName(AutoLoginPath)))
                                    Directory.CreateDirectory(Path.GetDirectoryName(AutoLoginPath));
                                using (BinaryWriter bw = new BinaryWriter(File.Open(AutoLoginPath, FileMode.Create)))
                                {
                                    bw.Write(email);
                                    bw.Write(md5 ? password : DBConnection.CreateMD5(password));
                                }
                            }
                            new MainWindow().Show();
                            Close();
                        }
                        else
                        {
                            Error.Text = "Algo ha salido mal, vuelve a intentarlo.";
                            Error.Visibility = Visibility.Visible;
                        }
                        break;
                }
        }
    }
}
