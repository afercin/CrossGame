using Cross_Game.DataManipulation;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Cross_Game.Connection
{
    class RTDPClient : RTDProtocol
    {
        public event CursorShangedEventHandler CursorShapeChanged;
        public event ImageBuiltEventHandler ImageBuilt;
        public event ReconectingEventHandler Reconnecting;
        
        private Thread receivePetitionThread;
        private Thread receiveDataThread;
        private Socket petitionsSocket;
        private Socket dataSocket;
        private Dictionary<int, ScreenImage> images;
        private byte skipImage;
        private string serverIP;

        public RTDPClient() : base()
        {
        }

        public void Start(ComputerData computerData)
        {
            int err = 0;
            bool connected = false;

            try
            {
                ConnectionUtils.GetComputerNetworkInfo(out string localIP, out string publicIP, out string mac); Computer = computerData;

                serverIP = Computer.PublicIP == publicIP ? Computer.LocalIP : Computer.PublicIP;

                LogUtils.AppendLogHeader(LogUtils.ClientConnectionLog);
                LogUtils.AppendLogText(LogUtils.ClientConnectionLog, $"Intentando conectar con {serverIP}:{Computer.Tcp}");

                if (ConnectionUtils.Ping(serverIP))
                {
                    Thread connectedThread = new Thread(() =>
                    {

                        petitionsSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                        while (!connected && err < 5)
                        {
                            LogUtils.AppendLogText(LogUtils.ClientConnectionLog, $"Intento de conexión {err + 1} de 5...");
                            try
                            {
                                petitionsSocket.Connect(new IPEndPoint(IPAddress.Parse(serverIP), Computer.Tcp));
                                connected = true;
                            }
                            catch (SocketException e)
                            {
                                LogUtils.AppendLogWarn(LogUtils.ClientConnectionLog, $"No se ha logrado establecer la conexión ({e.SocketErrorCode}).");
                                if (e.SocketErrorCode == SocketError.TimedOut)
                                    err = 5;
                                err++;
                            }
                            catch (ObjectDisposedException)
                            {
                                LogUtils.AppendLogWarn(LogUtils.ClientConnectionLog, $"Se ha cerrado la conexión mientras se intentaba conectar al servidor.");
                                err = 5;
                            }
                        }
                    });
                    connectedThread.IsBackground = true;
                    connectedThread.Start();

                    if (!connectedThread.Join(10000))
                        if (err == 0)
                        {
                            LogUtils.AppendLogError(LogUtils.ClientConnectionLog, "No se ha logrado establecer conexión (TimedOut).");
                            connectedThread.Abort();
                        }
                        else
                            connectedThread.Join();

                    if (connected)
                    {
                        LogUtils.AppendLogOk(LogUtils.ClientConnectionLog, "Se ha conseguido establecer conexión con el servidor.");
                        LogUtils.AppendLogText(LogUtils.ClientConnectionLog, "Mandando credenciales al equipo servidor...");

                        /****** Mandar credenciales y esperar confirmación *******/

                        byte[] buffer = new byte[] { 0 };
                        SendBuffer(petitionsSocket, Encoding.ASCII.GetBytes(mac));
                        connectedThread = new Thread(() =>
                        {
                            try
                            {
                                ReceiveBuffer(petitionsSocket, out buffer, out int bufferSize);
                            }
                            catch (SocketException)
                            {
                                LogUtils.AppendLogWarn(LogUtils.ClientConnectionLog, "Se ha cerrado el socket antes de recibir una respuesta del servidor.");
                            }
                        });
                        connectedThread.IsBackground = true;

                        connectedThread.Start();
                        if (!connectedThread.Join(10000))
                        {
                            LogUtils.AppendLogError(LogUtils.ClientConnectionLog, "No se ha obtenido la respuesta del servidor (TimedOut).");
                            connectedThread.Abort();
                        }

                        /*********************************************************/

                        if (((Petition)buffer[0]) == Petition.ConnectionAccepted)
                        {
                            LogUtils.AppendLogOk(LogUtils.ClientConnectionLog, "Conexión establecida con éxito.");
                            Init();
                            LogUtils.AppendLogOk(LogUtils.ClientConnectionLog, "Conexión de datos establecida, fin de la configuración.");
                        }
                        else
                        {
                            if (buffer[0] != 0)
                                LogUtils.AppendLogWarn(LogUtils.ClientConnectionLog, "La conexión ha sido rechazada por el servidor.");
                            Stop();
                        }
                    }
                    else if (err == 5)
                    {
                        LogUtils.AppendLogError(LogUtils.ClientConnectionLog, "El servidor no responde.");
                        Stop();
                    }
                }
                else
                {
                    if (ConnectionUtils.HasInternetConnection())
                        LogUtils.AppendLogError(LogUtils.ClientConnectionLog, "EL servidor no es alcanzable desde el cliente.");
                    else
                        LogUtils.AppendLogError(LogUtils.ClientConnectionLog, "El cliente ha perdido la conexión a internet.");
                    Stop();
                }
            }
            catch (InternetConnectionException)
            {
                LogUtils.AppendLogError(LogUtils.ClientConnectionLog, "No se tiene acceso a internet, el proceso no puede continuar.");
                Stop();
            }            
        }

        protected override void Init()
        {
            IsConnected = true;

            images = new Dictionary<int, ScreenImage>();
            skipImage = 255;

            for (int i = 1; i <= CacheImages; i++)
            {
                images[i] = new ScreenImage(450000);
            }

            dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            dataSocket.Bind(new IPEndPoint(IPAddress.Any, Computer.Udp));

            receivePetitionThread = new Thread(() => ReceivePetition(petitionsSocket));
            receivePetitionThread.IsBackground = true;
            receivePetitionThread.Start();

            receiveDataThread = new Thread(ReceiveData);
            receiveDataThread.IsBackground = true;
            receiveDataThread.Start();
        }

        public override void Stop()
        {
            LogUtils.AppendLogText(LogUtils.ClientConnectionLog, "Cerrando la conexión...");

            IsConnected = false;

            receivePetitionThread?.Abort();
            receiveDataThread?.Abort();

            petitionsSocket?.Close();
            dataSocket?.Close();

            LogUtils.AppendLogFooter(LogUtils.ClientConnectionLog);
        }

        public void Restart(bool r = false)
        {
            if (r)
                Reconnecting.Invoke(this, new ReconnectingEventArgs(true));
            else
            {
                LogUtils.AppendLogText(LogUtils.ClientConnectionLog, "Reiniciando conexión...");
                Stop();
                DBConnection.UpdateComputerInfo(Computer);
                Start(Computer);
            }
        }

        protected override void ReceivePetition(Socket s, byte[] buffer)
        {
            Petition petition = (Petition)buffer[0];
            switch (petition)
            {
                //case Petition.EndConnetion:
                //    Close();
                //    break;
                case Petition.SetWaveFormat:
                    WasapiLoopbackCapture cosa = new WasapiLoopbackCapture();
                    audio = new Audio();
                    audio.StartPlayer(cosa.WaveFormat);
                    break;
                default:
                    throw new Exception();
            }
        }

        private void ReceiveData()
        {
            try
            {
                while (IsConnected)
                {
                    byte[] data = new byte[MaxPacketSize];
                    int dataSize;

                    dataSize = dataSocket.Receive(data, MaxPacketSize, 0);

                    if (data[0] == 0x00) // Nuevo audio
                    {
                        audio?.PlayAudio(data);
                    }
                    else if (data[0] < 0xFF) // Nueva imagen
                    {
                        byte img = data[0];
                        if (dataSize == 5) // Se empieza a transmitir una nueva imagen
                        {
                            images[img].currentSize = 0;
                            images[img].imageSize = BitConverter.ToInt32(data, 1);
                        }
                        else // Agregar buffer a la imagen correspondiente
                        {
                            try
                            {
                                if (img != skipImage && images[img].AppendBuffer(data, 1, dataSize - 1, data[0] & 0x0F))
                                {
                                    byte[] i = images[img].ImageBytes;
                                    Array.Resize(ref i, images[img].imageSize);
                                    ImageBuilt.Invoke(this, new ImageBuiltEventArgs(i));
                                    if (img == (skipImage + 5) % 254)
                                        skipImage = 255;
                                }
                            }
                            catch (KeyNotFoundException)
                            {
                                LogUtils.AppendLogError(LogUtils.ClientConnectionLog, $"No ha llegado a tiempo el paquete que inicializaba el fotograma nº{img}");
                                skipImage = img;
                            }
                            catch (ArgumentException)
                            {
                                LogUtils.AppendLogError(LogUtils.ClientConnectionLog, $"No ha llegado a tiempo el paquete que inicializaba el fotograma nº{img}");
                                skipImage = img;
                            }
                        }                            
                    }
                    else // Nueva forma del cursor
                    {
                        CursorShapeChanged.Invoke(this, new CursorShangedEventArgs((CursorShape)data[1]));
                    }
                }
            }
            catch (SocketException e)
            {
                LogUtils.AppendLogError(LogUtils.ClientConnectionLog, e.Message + $" ({e.SocketErrorCode})");
            }
        }

        private void SendPetition(byte[] petition) => SendBuffer(petitionsSocket, petition);

        public void SendMousePosition(Point position, Size RenderSize)
        {
            byte[] petition = new byte[9];

            petition[0] = Convert.ToByte(Petition.MouseMove);
            BitConverter.GetBytes(Convert.ToSingle(position.X * 100.0 / RenderSize.Width)).CopyTo(petition, 1);
            BitConverter.GetBytes(Convert.ToSingle(position.Y * 100.0 / RenderSize.Height)).CopyTo(petition, 5);

            SendPetition(petition);
        }

        public void SendMouseButton(MouseButton mouseButton, bool isPressed)
        {
            switch (mouseButton)
            {
                case MouseButton.Left: SendOtherEvents(isPressed ? Petition.MouseLButtonDown : Petition.MouseLButtonUp); break;
                case MouseButton.Right: SendOtherEvents(isPressed ? Petition.MouseRButtonDown : Petition.MouseRButtonUp); break;
                case MouseButton.Middle: SendOtherEvents(isPressed ? Petition.MouseMButtonDown : Petition.MouseMButtonUp); break;
            }
        }

        public void SendMouseScroll(int delta)
        {
            byte[] petition = new byte[5];

            petition[0] = Convert.ToByte(Petition.MouseWheel);
            BitConverter.GetBytes(delta).CopyTo(petition, 1);

            SendPetition(petition);
        }

        public void SendKey(Key key, bool isPressed)
        {
            byte[] petition = new byte[2];

            petition[0] = Convert.ToByte(isPressed ? Petition.KeyboardKeyDown : Petition.KeyboardKeyUp);
            petition[1] = Convert.ToByte(key);

            SendPetition(petition);
        }

        public void SendOtherEvents(Petition petition) => SendPetition(new byte[] { Convert.ToByte(petition) });

        private class ScreenImage
        {
            public byte[] ImageBytes { get; set; }

            public int imageSize;
            public int currentSize;

            public ScreenImage(int size)
            {
                ImageBytes = new byte[size];
                imageSize = size;
                currentSize = 0;
            }

            public bool AppendBuffer(byte[] buffer, int offset, int bufferSize, int bufferIndex)
            {
                Array.Copy(buffer, offset, ImageBytes,/**/ currentSize /**bufferIndex * (MaxPacketSize - 1) /**/, bufferSize);
                currentSize += bufferSize;
                return currentSize == imageSize;
            }
        }
    }
}
