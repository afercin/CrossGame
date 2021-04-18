using Cross_Game.Connection;
using Cross_Game.DataManipulation;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace Cross_Game.Windows
{
    /// <summary>
    /// Lógica de interacción para PruebaSockets.xaml
    /// </summary>
    public partial class PruebaSockets : Window
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
        private RTDPController server;

        public PruebaSockets()
        {
            InitializeComponent();
            client = null;
            server = null;
        }
        private void Client_Click(object sender, RoutedEventArgs e)
        {
            if (client == null || !client.IsConnected)
            {
                client = new RTDPController(3030, 3031, "192.168.1.2");
                client.ImageBuilt += Client_ImageBuilt;
                client.CursorShapeChanged += Client_CursorShapeChanged;
            }
            else
            {
                client.Dispose();
                client = null;
            }
        }

        private void Server_Click(object sender, RoutedEventArgs e)
        {
            if (server == null || !server.IsConnected)
                server = new RTDPController(3030, 3031, 1, 20);
            else
            {
                server.Dispose();
                server = null;
            }
        }

        private void Client_ImageBuilt(object sender, ImageBuiltEventArgs e)
        {
            Dispatcher.Invoke(() => ClientDisplay.Source = Screen.BytesToScreenImage(e.Image));
        }

        private void Client_CursorShapeChanged(object sender, CursorShangedEventArgs e)
        {
            Dispatcher.Invoke(() => ClientDisplay.Cursor = cursors[e.CursorShape]);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            client?.Dispose();
            server?.Dispose();
        }

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
    }
}
