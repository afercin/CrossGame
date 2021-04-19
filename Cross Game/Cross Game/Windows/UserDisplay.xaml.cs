using Cross_Game.Connection;
using Cross_Game.DataManipulation;
using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace Cross_Game.Windows
{
    /// <summary>
    /// Lógica de interacción para PruebaSockets.xaml
    /// </summary>
    public partial class UserDisplay : Window
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

        private RTDPController client;
        private Timer framerate;

        public UserDisplay()
        {
            InitializeComponent();
            framerate = new Timer(500);
            framerate.Elapsed += FrameRate_Tick;
            client = null;
        }

        Random r = new Random();
        private void FrameRate_Tick(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => FPS.Text = r.Next(50, 60).ToString());
        }

        public void StartTransmission(int tcpPort, int udpPort, string remoteIP)
        {
            if (client != null && client.IsConnected)
            {
                client.Dispose();
                client.ImageBuilt -= Client_ImageBuilt;
                client.CursorShapeChanged -= Client_CursorShapeChanged;
                framerate.Stop();
            }
            client = new RTDPController(tcpPort, udpPort, remoteIP);
            client.ImageBuilt += Client_ImageBuilt;
            client.CursorShapeChanged += Client_CursorShapeChanged;
            framerate.Start();

            ShowDialog();
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

        private void Client_ImageBuilt(object sender, ImageBuiltEventArgs e) => Dispatcher.Invoke(() => ClientDisplay.Source = Screen.BytesToScreenImage(e.Image));

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

        private void ClientDisplay_KeyDown(object sender, KeyEventArgs e)
        {
            if (client != null && client.IsConnected)
                client.SendKey(e.Key, e.IsDown);
        }

        private void ClientDisplay_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (client != null && client.IsConnected)
                client.SendMouseScroll(e.Delta);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            framerate.Stop();
            client?.Dispose();
        }
    }
}
