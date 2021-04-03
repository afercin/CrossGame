using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Cross_Game.Client
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class ClientWindow : Window
    {
        private KeyboardHook KeyboardHook;

        public ClientWindow()
        {

            InitializeComponent();

            ClientInit();

            KeyboardHook = new KeyboardHook();
            KeyboardHook.KeyPress += KeyboardHook_KeyPress;
        }

        private void IniciarEscucha_Click(object sender, RoutedEventArgs e)
        {
            ServerIP = IP.Text;
            TcpPort = 3030;
            UdpPort = 8554;
            Start();
        }

        private void DetenerEscucha_Click(object sender, RoutedEventArgs e) => Stop();

        private void KeyboardHook_KeyPress(object sender, KeyEventArgs e) => SendKey((byte)e.Key, e.IsKeyDown ? Actions.KeyboardKeyDown : Actions.KeyboardKeyUp);
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            KeyboardHook.Dispose();
            Stop();
        }

        #region Client Operations

        private Thread recvThread;
        private TCPClient TCPClient;
        private ClientDisplay Display;

        public string ServerIP { get; set; }
        public int TcpPort { get; set; }
        public int UdpPort { get; set; }
        public bool IsRunning { get => TCPClient != null; }

        private void ClientInit()
        {
            TCPClient = null;
            recvThread = null;
        }

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

        private void Start()
        {
            if (!IsRunning)
            {
                new Thread(() =>
                {
                    TCPClient = new TCPClient(ServerIP, TcpPort);
                    bool connected = false;
                    int err = 0;
                    Thread connectedThread = new Thread(() =>
                    {
                        try
                        {
                            while (!connected && err < 5)
                            {
                                Console.WriteLine("Intentando conectar con el servidor.\r\nIntento " + (err + 1) + " de 5...");
                                connected = TCPClient.Start();
                                err++;
                            }
                        }
                        catch { }
                    });

                    connectedThread.IsBackground = true;
                    connectedThread.Start();

                    if (!connectedThread.Join(5000))
                        if (err == 0)
                            connectedThread.Abort();
                        else
                            connectedThread.Join();

                    if (connected)
                    {
                        if (Display == null)
                            Dispatcher.Invoke(() =>
                            {
                                Display = new ClientDisplay();
                                Display.TCPClient = TCPClient;
                                Display.Visibility = Visibility.Visible;
                                Display.Closed += Display_Closed;
                            });
                        Display.SetLogText("");
                        Display.Start(UdpPort);

                        recvThread = new Thread(ReceiveThread);
                        recvThread.IsBackground = true;
                        recvThread.Start();
                    }
                    else
                    {
                        if (err > 0)
                            Console.WriteLine("No se ha podido establecer la conexión.");
                        else
                            Console.WriteLine("El servidor parece no estar accesible.");
                        Stop();
                    }
                }).Start();
            }
        }

        private void Display_Closed(object sender, EventArgs e)
        {
            Display = null;
            Stop();
        }

        private void Stop(bool closeDiplay = true)
        {
            if (Display != null && closeDiplay)
            {
                if (Dispatcher.CheckAccess())
                {
                    Display.TCPClient = null;
                    Display.Close();
                }
                else
                    Dispatcher.Invoke(() =>
                    {
                        Display.TCPClient = null;
                        Display.Close();
                    });
                Display = null;
            }
            
            TCPClient?.Close();
            TCPClient = null;

            recvThread?.Abort();
        }

        private void Restart()
        {
            Stop(false);
            Start();
        }

        private void ReceiveThread()
        {
            try
            {
                while (IsRunning)
                {
                    byte[] data = TCPClient.ReceiveData();
                    if (data != null)
                    {
                        Actions action = (Actions)data[0];
                        switch (action)
                        {
                            case Actions.EndConnetion: break;
                            case Actions.CursorChanged:
                                Dispatcher.Invoke(() =>
                                {
                                    try
                                    {
                                        Display.SetCursorShape(cursors[(CursorShape)data[1]]);
                                        //Cursor = (Cursor)typeof(Cursors).GetProperty(((CursorShape)data[1]).ToString()).GetValue(Cursor, null);
                                    }
                                    catch
                                    {
                                        Console.WriteLine("Cursor shape not found: " + data[1].ToString());
                                    }
                                });
                                break;
                            case Actions.EmulatorInfo: break;
                            case Actions.GameInfo: break;
                        }
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch
            {
                Display.SetLogText("Se ha perdido la conexión con el servidor");
                new Thread(() => Restart()).Start();
            }
        }

        private void SendKey(byte key, Actions action) => TCPClient?.SendData(new byte[] { Convert.ToByte(action), key });

        #endregion
    }
}