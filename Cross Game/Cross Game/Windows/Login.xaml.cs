using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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

            LogUtils.AppendLogHeader(LogUtils.LoginLog);
            try
            {
                LogUtils.AppendLogOk(LogUtils.LoginLog, "Se ha detectado el fichero de autologin, comprobando las credenciales al usuario...");

                string email, password;

                using (FileStream fileStream = new FileStream(AutoLoginPath, FileMode.Open))
                {
                    RememberMe.IsChecked = true;
                    using (Aes aes = Aes.Create())
                    {
                        byte[] iv = new byte[aes.IV.Length];
                        int numBytesToRead = aes.IV.Length;
                        int numBytesRead = 0;

                        while (numBytesToRead > 0)
                        {
                            int n = fileStream.Read(iv, numBytesRead, numBytesToRead);
                            if (n == 0) break;

                            numBytesRead += n;
                            numBytesToRead -= n;
                        }

                        using (CryptoStream cryptoStream = new CryptoStream(fileStream, aes.CreateDecryptor(GetVolumeInfo(), iv), CryptoStreamMode.Read))
                        {
                            using (StreamReader decryptReader = new StreamReader(cryptoStream))
                            {
                                email = decryptReader.ReadLine();
                                password = decryptReader.ReadLine();
                            }
                        }
                    }
                }

                Email.Text = email;
                Password.Password = password;

                CheckLogin(email, password);
            }
            catch (IOException)
            {
                LogUtils.AppendLogWarn(LogUtils.LoginLog, "No existe el fichero de autologin, se le solicitarán las credenciales al usuario.");
            }
            catch (Exception)
            {
                LogUtils.AppendLogWarn(LogUtils.LoginLog, "Error al leer el fichero de autologin, se le solicitarán las credenciales al usuario.");
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
                Error.Visibility = Visibility.Hidden;
        }

        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (Password.Password != string.Empty && Password.Password != watermakPassword && passwordWatermark)
                passwordWatermark = false;
            if (Password.Password == string.Empty && !passwordWatermark)
                passwordWatermark = true;
            if (Error != null && Error.Visibility == Visibility.Visible && !passwordWatermark)
                Error.Visibility = Visibility.Hidden;
        }

        private void Error_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Error.Visibility == Visibility.Visible)
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
        private void CheckLogin(string email, string password)
        {
            if (email != watermakEmail && password != watermakPassword && Checking.Visibility != Visibility.Visible)
            {
                Error.Visibility = Visibility.Hidden;
                Checking.Visibility = Visibility.Visible;

                Task.Run(()=>
                {
                    LogUtils.AppendLogText(LogUtils.LoginLog, "Comprobando las credenciales introducidas...");
                    
                    UserData currentUser = DBConnection.CheckLogin(email, password);

                    if (currentUser == null)
                    {
                        LogUtils.AppendLogError(LogUtils.LoginLog, "Se ha producido un error al conectar con la base de datos.");
                        Dispatcher.Invoke(() =>
                        {
                            Error.Text = "Error de conexión.";
                            Error.Visibility = Visibility.Visible;
                        });
                    }
                    else if (currentUser.ID == 0)
                    {
                        LogUtils.AppendLogWarn(LogUtils.LoginLog, "Las credenciales introducidas no concuerdan con las de ningún usuario.");
                        Dispatcher.Invoke(() =>
                        {
                            Error.Text = "Email o contraseña incorrectos.";
                            Error.Visibility = Visibility.Visible;
                        });
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            LogUtils.AppendLogOk(LogUtils.LoginLog, "Las credenciales introducidas son correctas.");
                            if (RememberMe.IsChecked == true)
                            {
                                LogUtils.AppendLogText(LogUtils.LoginLog, "\"Remember me\" estaba marcado por lo que se procede guardar las credenciales en el fichero de autologin.");

                                if (!Directory.Exists(Path.GetDirectoryName(AutoLoginPath)))
                                    Directory.CreateDirectory(Path.GetDirectoryName(AutoLoginPath));

                                try
                                {
                                    using (FileStream fileStream = new FileStream(AutoLoginPath, FileMode.Create))
                                    {
                                        using (Aes aes = Aes.Create())
                                        {
                                            aes.Key = GetVolumeInfo();

                                            byte[] iv = aes.IV;
                                            fileStream.Write(iv, 0, iv.Length);

                                            using (CryptoStream cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                                            {
                                                using (StreamWriter encryptWriter = new StreamWriter(cryptoStream))
                                                {
                                                    encryptWriter.WriteLine(email);
                                                    encryptWriter.WriteLine(password);
                                                }
                                            }
                                        }
                                    }
                                    LogUtils.AppendLogOk(LogUtils.LoginLog, "Fichero autologin creado con éxito.");
                                }
                                catch (Exception ex)
                                {
                                    LogUtils.AppendLogError(LogUtils.LoginLog, "Ha ocurrido un error al generar el fichero: " + ex);
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
                        });                        
                    }
                    Dispatcher.Invoke(() => Checking.Visibility = Visibility.Hidden);
                });                
            }
        }

        private byte[] GetVolumeInfo()
        {
            byte[] SN = Encoding.ASCII.GetBytes(GetWindowDiskSN() + Environment.MachineName);
            Array.Resize(ref SN, 16);
            return SN;
        }

        public static string GetWindowDiskSN()
        {
            try
            {
                string windowsDriveLetter = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)).Split('\\')[0];
                using (var partitions = new ManagementObjectSearcher("ASSOCIATORS OF {Win32_LogicalDisk.DeviceID='" + windowsDriveLetter + "'} WHERE ResultClass=Win32_DiskPartition"))
                    foreach (var partition in partitions.Get())
                        using (var drives = new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + partition["DeviceID"] + "'} WHERE ResultClass=Win32_DiskDrive"))
                            foreach (var drive in drives.Get())
                                return (string)drive["SerialNumber"];
            }
            catch
            {
                LogUtils.AppendLogError(LogUtils.LoginLog, "404");
                using (var disks = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive"))
                    foreach (var disk in disks.Get())
                        return disk["SerialNumber"].ToString();
            }
            
            return "<unknown>";
        }

        private void Window_Closed(object sender, EventArgs e) => LogUtils.AppendLogFooter(LogUtils.LoginLog);
    }
}
