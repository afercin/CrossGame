using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Cross_Game
{
    /// <summary>
    /// Lógica de interacción para LoginRegister.xaml
    /// </summary>
    public partial class LoginRegister : Window
    {
        private readonly Brush White = new SolidColorBrush(Colors.White);

        private const string watermakEmail = "email@example.com";
        private const string watermakPassword = "email@example.c";
        private bool emailWatermark;
        private bool passwordWatermark;

        public LoginRegister()
        {
            InitializeComponent();
            
            emailWatermark = true;
            passwordWatermark = true;
            Error.Visibility = Visibility.Hidden;
        }

        private void MainWindow_OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
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

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("http://localhost/codeigniter3/register");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (DBConnect.CheckLogin(Email.Text, Password.Password))
            {
                case -1:
                    Error.Text = "Error de conexión.";
                    Error.Visibility = Visibility.Visible;
                    break;
                case 0:
                    Error.Text = "Email o contraseña incorrectos";
                    Error.Visibility = Visibility.Visible;
                    break;
                default:

                    break;
            }
            Error.Text = "";
        }

        private void Error_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if((sender as TextBlock).Visibility == Visibility.Visible)
            {
                Email.Foreground = Error.Foreground;
                Password.Foreground = Error.Foreground;
            }
            else
            {
                Email.Foreground = White;
                Password.Foreground = White;
            }
        }

        private void Error_TextInput(object sender, TextCompositionEventArgs e)
        {
            MessageBox.Show("hola");

        }
    }
}
