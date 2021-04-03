using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace Cross_Game.Server
{
    class Server
    {
        private Thread recvThread;
        private TCPServer TCPServer;
        private UDPSender UDPSender;

        private DispatcherTimer cursorTimer;
        private IntPtr currentCursor;

        public bool IsRunning { get => TCPServer != null; }

        public Server()
        {
            cursorTimer = new DispatcherTimer();
            cursorTimer.Interval = TimeSpan.FromMilliseconds(20);
            cursorTimer.Tick += SendCursorShape;

            currentCursor = Cursors.Default.Handle;

            TCPServer = null;
            UDPSender = null;
        }

        public void Start()
        {
            if (!IsRunning)
                new Thread(() =>
                {
                    TCPServer = new TCPServer(ConnectionUtils.TcpPort);

                    if (TCPServer.Start())
                    {
                        UDPSender = new UDPSender(TCPServer.ClientIP.Address.ToString(), ConnectionUtils.UdpPort);
                        UDPSender.Display.Width = (int)SystemParameters.PrimaryScreenWidth;
                        UDPSender.Display.Height = (int)SystemParameters.PrimaryScreenHeight;

                        recvThread = new Thread(ReceiveThread);
                        recvThread.Start();

                        cursorTimer.Start();
                    }
                }).Start();
        }

        public void Stop()
        {
            cursorTimer?.Stop();
            recvThread?.Abort();

            TCPServer?.Close();
            TCPServer = null;
            UDPSender?.Close();

        }

        public void Restart()
        {
            Stop();
            Start();
        }

        private void ReceiveThread()
        {
            try
            {
                while (IsRunning)
                {
                    byte[] data = TCPServer.ReceiveData();
                    if (data != null)
                    {
                        Actions action = (Actions)data[0];
                        switch (action)
                        {
                            case Actions.EndConnetion: throw new Exception("Connection Lost");
                            case Actions.IncreaseDelay: ConnectionUtils.Delay += data[1]; break;
                            case Actions.DecreaseDelay: ConnectionUtils.Delay -= data[1]; break;
                            case Actions.MouseMove:
                                float xPos = GetPosition(data, 1);
                                float yPos = GetPosition(data, 5);
                                MouseSimulator.Move(Convert.ToInt32(SystemParameters.PrimaryScreenWidth * xPos / 100), Convert.ToInt32(SystemParameters.PrimaryScreenHeight * yPos / 100));
                                break;
                            case Actions.MouseLButtonDown:
                            case Actions.MouseRButtonDown:
                            case Actions.MouseMButtonDown:
                            case Actions.MouseLButtonUp:
                            case Actions.MouseRButtonUp:
                            case Actions.MouseMButtonUp:
                                MouseSimulator.Button(action);
                                break;
                            case Actions.MouseWheel:
                                MouseSimulator.Wheel(BitConverter.ToInt32(data, 1));
                                break;
                            case Actions.KeyboardKeyDown:
                            case Actions.KeyboardKeyUp:
                                DXKeyboardSimulator.SendKey(data[1], action);
                                break;
                        }
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch
            {
                new Thread(() => Restart()).Start();
            }
        }

        private float GetPosition(byte[] data, int offset)
        {
            byte[] value = new byte[4];
            for (int i = 0; i < 4; i++)
                value[i] = data[i + offset];
            offset += 4;
            return BitConverter.ToSingle(value, 0);
        }

        private void SendCursorShape(object sender, EventArgs e)
        {
            CURSORINFO pci;
            pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
            GetCursorInfo(out pci);

            if (currentCursor != pci.hCursor)
            {
                if ((int)pci.hCursor <= 0x1001F)
                {
                    Console.WriteLine(pci.hCursor.ToString());
                    TCPServer.SendData(new byte[] { (byte)Actions.CursorChanged, (byte)pci.hCursor });
                    currentCursor = pci.hCursor;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CURSORINFO
        {
            public int cbSize;        // Specifies the size, in bytes, of the structure. 
                                      // The caller must set this to Marshal.SizeOf(typeof(CURSORINFO)).
            public int flags;         // Specifies the cursor state. This parameter can be one of the following values:
                                      //    0             The cursor is hidden.
                                      //    CURSOR_SHOWING    The cursor is showing.
            public IntPtr hCursor;          // Handle to the cursor. 
            public POINT ptScreenPos;       // A POINT structure that receives the screen coordinates of the cursor. 
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorInfo(out CURSORINFO pci);
    }
}
