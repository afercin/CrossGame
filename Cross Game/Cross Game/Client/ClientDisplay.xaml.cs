using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cross_Game;

namespace Cross_Game.Client
{
    /// <summary>
    /// Lógica de interacción para ClientDisplay.xaml
    /// </summary>
    public partial class ClientDisplay : Window
    {
        private readonly DispatcherTimer fpsTimer;
        private UDPListener UDPListener;
        private int fps;
        
        public TCPClient TCPClient;

        public Visibility ShowFPS
        {
            get => FPS.Visibility;
            set => FPS.Visibility = value;
        }

        public ClientDisplay()
        {
            InitializeComponent();

            UDPListener = null;

            fpsTimer = new DispatcherTimer();
            fpsTimer.Interval = TimeSpan.FromSeconds(1);
            fpsTimer.Tick += Timer_Tick;
        }

        public void Start(int udpPort)
        {
            UDPListener = new UDPListener(udpPort);
            UDPListener.ImageBuilt += Listener_ImageBuilt;
            fpsTimer.Start();
        }

        public void Stop()
        {
            if (UDPListener != null)
            {
                UDPListener.ImageBuilt -= Listener_ImageBuilt;
                UDPListener.Close();
                UDPListener = null;
            }

            fpsTimer.Stop();
        }
        
        public void SetCursorShape(Cursor newCursor) => Cursor = newCursor;

        delegate void ParametrizedMethodInvoker5(string text);
        public void SetLogText(string text)
        {
            if (!Dispatcher.CheckAccess())
                Dispatcher.Invoke(new ParametrizedMethodInvoker5(SetLogText), text);
            else
            {
                LogText.Visibility = text.Length > 0 ? Visibility.Visible : Visibility.Hidden;
                LogText.Text = text;
            }

        }

        #region Auxiliar functions

        private BitmapImage ByteToImage(byte[] data)
        {
            BitmapImage bitmap = new BitmapImage();
            using (MemoryStream stream = new MemoryStream(data))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }
            return bitmap;
        }

        private void SendMousePosition(double xPosition, double yPosition, Size RenderSize)
        {
            byte[] data = new byte[9];

            data[0] = Convert.ToByte(Actions.MouseMove);
            BitConverter.GetBytes(Convert.ToSingle(xPosition * 100.0 / RenderSize.Width)).CopyTo(data, 1);
            BitConverter.GetBytes(Convert.ToSingle(yPosition * 100.0 / RenderSize.Height)).CopyTo(data, 5);

            TCPClient?.SendData(data);
        }

        private void SendMouseWheel(int delta)
        {
            byte[] data = new byte[5];

            data[0] = Convert.ToByte(Actions.MouseWheel);
            BitConverter.GetBytes(delta).CopyTo(data, 1);

            TCPClient?.SendData(data);
        }

        private void SendMouseButton(Actions action) => TCPClient?.SendData(new byte[] { Convert.ToByte(action) });

        #endregion

        #region Window events

        private void Listener_ImageBuilt(object sender, ImageBuiltEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    Display.Source = ByteToImage(e.imageBytes);
                    fps++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error interno: " + ex.Message);
                }
            }));
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            FPS.Text = fps.ToString();
            fps = 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MinHeight = MaxHeight = Height;
            MinWidth = MaxWidth = Width;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (TCPClient == null || MessageBox.Show("¿Realmente desea cerrar la transimisión?", "Cerrar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                Stop();
            else
                e.Cancel = true;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left: SendMouseButton(Actions.MouseLButtonDown); break;
                case MouseButton.Right: SendMouseButton(Actions.MouseRButtonDown); break;
                case MouseButton.Middle: SendMouseButton(Actions.MouseMButtonDown); break;
                    //case MouseButton.XButton1: client.SendMouseButton(Client.Actions.MouseLButtonDown); break;
                    //case MouseButton.XButton2: client.SendMouseButton(Client.Actions.MouseLButtonDown); break;
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left: SendMouseButton(Actions.MouseLButtonUp); break;
                case MouseButton.Right: SendMouseButton(Actions.MouseRButtonUp); break;
                case MouseButton.Middle: SendMouseButton(Actions.MouseMButtonUp); break;
                    //case MouseButton.XButton1: client.SendMouseButton(Client.Actions.MouseLButtonUp); break;
                    //case MouseButton.XButton2: client.SendMouseButton(Client.Actions.MouseLButtonUp); break;
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(Display);
            SendMousePosition(p.X, p.Y, Display.RenderSize);
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e) => SendMouseWheel(e.Delta);

        #endregion
    }
}
