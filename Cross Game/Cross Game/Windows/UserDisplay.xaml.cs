using Cross_Game.Connection;
using Cross_Game.DataManipulation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

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
            { CursorShape.ArrowCD, Cursors.ArrowCD },
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
                        Dispatcher.Invoke(() => ClientDisplay.Source = Screen.BytesToScreenImage(latestImage));
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

        public void StartTransmission(ComputerData serverComputer)
        {
            if (client != null && client.IsConnected)
            {
                client.Stop();
                client.ImageBuilt -= Client_ImageBuilt;
                client.CursorShapeChanged -= Client_CursorShapeChanged;
                framerate.Stop();
                paint.Abort();
            }

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
                client = new RTDPClient();
                client.ImageBuilt += Client_ImageBuilt;
                client.CursorShapeChanged += Client_CursorShapeChanged;
                client.Reconnecting += Client_Reconnecting;
                client.Start(serverComputer);
            }, $"Conectando con {serverComputer.Name}...");
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

        //private void Server_Click(object sender, RoutedEventArgs e)
        //{
        //    if (server == null || !server.IsConnected)
        //        server = new RTDPController(3030, 3031, 1, 20);
        //    else
        //    {
        //        server.Dispose();
        //        server = null;
        //    }
        //}

        DateTime time = DateTime.Now;

        private void Client_ImageBuilt(object sender, ImageBuiltEventArgs e)
        {
            latestImage = e.Image;
            frames++;
            Console.WriteLine((DateTime.Now - time).TotalMilliseconds);
            time = DateTime.Now;
        }

        private void Client_CursorShapeChanged(object sender, CursorShangedEventArgs e) => Dispatcher.Invoke(() => ClientDisplay.Cursor = cursors[e.CursorShape]);

        private void ClientDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            if (client != null && client.IsConnected)
                client.SendMousePosition(e.GetPosition(ClientDisplay), ClientDisplay.RenderSize);
        }

        private void ClientDisplay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (client != null && client.IsConnected)
                client.SendMouseButton(e.ChangedButton, true);
        }

        private void ClientDisplay_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (client != null && client.IsConnected)
                client.SendMouseButton(e.ChangedButton, false);
        }

        private void ClientDisplay_KeyPress(object sender, KeyEventArgs e)
        {
            if (client != null && client.IsConnected && e.Key != Key.DeadCharProcessed && e.Key != Key.OemFinish)
                client.SendKey(e.Key, e.IsDown);
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
