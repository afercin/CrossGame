using RTDP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Cross_Game.Windows
{
    /// <summary>
    /// Lógica de interacción para PruebaSockets.xaml
    /// </summary>
    public partial class UserDisplay : Window, IDisposable
    {
        private readonly Dictionary<CursorShape, Cursor> cursors = new Dictionary<CursorShape, Cursor>
        {
            { CursorShape.None, Cursors.None },
            { CursorShape.Arrow, Cursors.Arrow },
            { CursorShape.IBeam, Cursors.IBeam },
            { CursorShape.ArrowCD, Cursors.AppStarting },
            { CursorShape.Cross, Cursors.Cross },
            { CursorShape.SizeNS, Cursors.SizeNS },
            { CursorShape.SizeWE, Cursors.SizeWE },
            { CursorShape.SizeNESW, Cursors.SizeNESW },
            { CursorShape.SizeNWSE, Cursors.SizeNWSE },
            { CursorShape.Hand, Cursors.Hand },
            { CursorShape.Wait, Cursors.Wait }
        };

        private RTDPClient client;
        private WaitingWindow waitingWindow;
        private Timer framerate;
        private System.Threading.Thread paint;
        private int frames;
        private byte[] latestImage;

        public UserDisplay()
        {
            InitializeComponent();
            framerate = new Timer(1000);
            framerate.Elapsed += FrameRate_Tick;
            client = null;
            latestImage = null;
        }

        private void PaintThread()
        {
            while (true)
            {
                Task.Run(() =>
                {
                    if (latestImage != null)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            BitmapImage bitmap = new BitmapImage();
                            using (MemoryStream stream = new MemoryStream(latestImage))
                            {
                                bitmap.BeginInit();
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.StreamSource = stream;
                                bitmap.EndInit();
                            }
                            ClientDisplay.Source = bitmap;
                        });
                    }                        
                    else
                    {
                        //default img
                    }
                });
                System.Threading.Thread.Sleep(1000 / 60);
            }
        }

        private void FrameRate_Tick(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                FPS.Text = frames.ToString();
                frames = 0;
            });
        }

        public void StartTransmission(ref ComputerData server)
        {
            ComputerData serverComputer = server;
            string password = string.Empty, passFile = Path.Combine(LogUtils.CrossGameFolder, $"{serverComputer.MAC}.password");
            bool error = false;
            try
            {
                LogUtils.AppendLogHeader(LogUtils.ClientConnectionLog);

                byte[] decryptPassword = Crypto.GetBytes(Crypto.CreateSHA256(CrossGameUtils.GetWindowDiskSN(LogUtils.LoginLog)));
                Array.Resize(ref decryptPassword, 32);
                LogUtils.AppendLogHeader(LogUtils.ClientConnectionLog);

                password = Crypto.ReadData(passFile, decryptPassword)[0];

                LogUtils.AppendLogOk(LogUtils.ClientConnectionLog, $"Se ha detectado el fichero con la contraseña de del equipo actual. Iniciando proceso de conexión...");
            }
            catch (IOException)
            {
                LogUtils.AppendLogWarn(LogUtils.ClientConnectionLog, "No existe el fichero con la contraseña de del equipo actual, se le solicitará al usuario.");
                password = string.Empty;
            }
            catch (Exception)
            {
                LogUtils.AppendLogWarn(LogUtils.ClientConnectionLog, "Error al leer el fichero con la contraseña de del equipo actual, se le solicitará al usuario.");
                File.Delete(passFile);
                password = string.Empty;
                error = true;
            }

            if (password == string.Empty)
                Dispatcher.Invoke(() => password = new PasswordWindow().GetPassword(serverComputer.MAC, serverComputer.Name, error));

            if (password != string.Empty)
            {
                frames = 0;
                framerate.Start();
                paint = new System.Threading.Thread(PaintThread);
                paint.IsBackground = true;
                paint.Start();

                waitingWindow = new WaitingWindow();
                waitingWindow.WaitStopped += (s, a) => Dispatcher.Invoke(() => Close());
                waitingWindow.WaitEnd += (s, a) => { if (!client.IsConnected) Dispatcher.Invoke(() => Close()); };
                waitingWindow.Wait(() =>
                {
                    client = new RTDPClient(password);
                    client.ImageBuilt += Client_ImageBuilt;
                    client.CursorShapeChanged += Client_CursorShapeChanged;
                    client.Reconnecting += Client_Reconnecting;
                    client.Start(ref serverComputer, DBConnection.UserEmail, DBConnection.UserPassword);
                }, $"Conectando con {serverComputer.Name}...");
            }
            else
                Close();
        }

        private void Client_Reconnecting(object sender, ReconnectingEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.Reconnecting)
                {
                    waitingWindow = new WaitingWindow();
                    waitingWindow.WaitStopped += (s, a) => { if (!client.IsConnected) Dispatcher.Invoke(() => Close()); };
                    waitingWindow.WaitEnd += (s, a) => { if (!client.IsConnected) Dispatcher.Invoke(() => Close()); };
                    waitingWindow.Wait(() =>
                    {
                        client.Restart();
                    }, $"Reconectando con el servidor...");
                }
                else
                    waitingWindow.Close();
            });
        }

        private void Client_ImageBuilt(object sender, ImageBuiltEventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            latestImage = e.Image;
            frames++;

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            Console.WriteLine("ImageBuilt: {0}ms",stopwatch.ElapsedMilliseconds);
        }

        private void Client_CursorShapeChanged(object sender, CursorChangedEventArgs e) => Dispatcher.Invoke(() => ClientDisplay.Cursor = cursors[e.CursorShape]);

        private void ClientDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            if (client != null && client.IsConnected)
            {
                Point position = e.GetPosition(ClientDisplay);
                Size displaySize = ClientDisplay.RenderSize;
                client.SendMousePosition(Convert.ToSingle(position.X * 100.0 / displaySize.Width), Convert.ToSingle(position.Y * 100.0 / displaySize.Height));
            }
        }

        private void ClientDisplay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (client != null && client.IsConnected)
                switch (e.ChangedButton)
                {
                    case MouseButton.Left: client.SendEvent(Petition.MouseLButtonDown); break;
                    case MouseButton.Right: client.SendEvent(Petition.MouseRButtonDown); break;
                    case MouseButton.Middle: client.SendEvent(Petition.MouseMButtonDown); break;
                }
        }

        private void ClientDisplay_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (client != null && client.IsConnected)
                switch (e.ChangedButton)
                {
                    case MouseButton.Left: client.SendEvent(Petition.MouseLButtonUp); break;
                    case MouseButton.Right: client.SendEvent(Petition.MouseRButtonUp); break;
                    case MouseButton.Middle: client.SendEvent(Petition.MouseMButtonUp); break;
                }
        }

        private void ClientDisplay_KeyPress(object sender, KeyEventArgs e)
        {
            if (client != null && client.IsConnected && !IsForbiddenKey(e.Key))
                client.SendKey((byte)e.Key, e.IsDown);
        }

        private bool IsForbiddenKey(Key key)
        {
            return key != Key.VolumeMute &&
                   key != Key.VolumeDown &&
                   key != Key.VolumeUp;
        }

        private void ClientDisplay_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (client != null && client.IsConnected)
                client.SendMouseScroll(e.Delta);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = client != null && client.IsConnected &&
                MessageBox.Show("¿Desea realmente cerrar la Transmisión?", "Cerrar la transmisión", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) != MessageBoxResult.Yes;
        }

        private void Window_Closed(object sender, EventArgs e) => Dispose();

        public void Dispose()
        {
            framerate.Dispose();
            paint.Abort();
            client?.Stop();
            waitingWindow?.Close();
        }
    }
}
