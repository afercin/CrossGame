using Cross_Game.DataManipulation;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Cross_Game.Connection
{
    class RTDPServer : RTDProtocol
    {
        private Dictionary<IPAddress, Client> clientSockets;
        private Thread listenThread;
        private Thread CaptureScreen;
        private Thread CheckCursorShape;
        private Socket listenSocket;
        private int frameRate;
        private UserData user;


        public RTDPServer() : base()
        {
        }

        public void Start(UserData currentUser)
        {
            if (!IsConnected)
                clientSockets = new Dictionary<IPAddress, Client>();

            user = currentUser;
            Computer = currentUser.localMachine;
            frameRate = 1000 / Computer.FPS;

            Computer.Status = 1;
            Computer.N_connections = 0;
            DBConnection.UpdateComputerInfo(Computer);

            listenThread = new Thread(ConnectionThread);
            listenThread.IsBackground = true;
            listenThread.Start();
        }

        private void ConnectionThread()
        {
            Socket tcpClientSocket;
            Socket udpClientSocket;
            IPEndPoint clientAddress;
            Thread receivePetitionThread;
            string clientMAC;
            int userPriority = 0;

            LogUtils.AppendLogHeader(LogUtils.ServerConnectionLog);
            LogUtils.AppendLogText(LogUtils.ServerConnectionLog, $"Comenzando a recibir clientes por el puerto {Computer.Tcp}");

            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(IPAddress.Any, Computer.Tcp));
            listenSocket.Listen(Computer.Max_connections - Computer.N_connections);
            try
            {
                while (Computer.N_connections < Computer.Max_connections) // diseñar alguna forma de recibir clientes permanentemente
                {
                    tcpClientSocket = listenSocket.Accept();
                    clientAddress = tcpClientSocket.RemoteEndPoint as IPEndPoint;
                    LogUtils.AppendLogText(LogUtils.ServerConnectionLog, $"Intento de conexión del cliente con IP {clientAddress.Address.ToString()}");
                    /***** Esperar y comprobar credenciales del usuario *****/

                    ReceiveBuffer(tcpClientSocket, out byte[] buffer, out int bufferSize);
                    clientMAC = Crypto.GetString(buffer, 0, bufferSize);

                    if (DBConnection.IsCorrectComputerIP(clientMAC, clientAddress.Address.ToString()))
                        userPriority = DBConnection.GetUserPriority(user, clientMAC);

                    /*********************************************************/
                    if (userPriority > 0)
                    {
                        LogUtils.AppendLogOk(LogUtils.ServerConnectionLog, $"Cliente {clientAddress.Address.ToString()} aceptado, procediendo a establecer canal de comunicación.");

                        SendBuffer(tcpClientSocket, new byte[] { Convert.ToByte(Petition.ConnectionAccepted) });

                        if (Computer.N_connections == 0)
                            Init();

                        SendWaveFormat(tcpClientSocket, new byte[] { 0 }); // TODO: Enviar waveformat

                        udpClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        udpClientSocket.Connect(new IPEndPoint(clientAddress.Address, Computer.Udp));

                        Client client = new Client()
                        {
                            mouse = userPriority == 2 ? new MouseSimulator() : null,
                            keyboard = new DXKeyboardSimulator(),
                            tcpSocket = tcpClientSocket,
                            udpSocket = udpClientSocket
                        };


                        clientSockets[clientAddress.Address] = client;

                        receivePetitionThread = new Thread(() => ReceivePetition(tcpClientSocket));
                        receivePetitionThread.IsBackground = true;
                        receivePetitionThread.Start();

                        Computer.N_connections++;

                        DBConnection.UpdateComputerInfo(Computer);

                        LogUtils.AppendLogOk(LogUtils.ServerConnectionLog, $"Cliente con IP {clientAddress.Address.ToString()} agregado con éxito.");
                    }
                    else
                    {
                        LogUtils.AppendLogWarn(LogUtils.ServerConnectionLog, $"No se acepta la conexión desde {clientAddress.Address.ToString()}, no es un usuario autorizado.");
                        SendBuffer(tcpClientSocket, new byte[] { Convert.ToByte(Petition.ConnectionRefused) });
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Recojo la excepcion que salta al abortar el thread para que no vaya al log.
            }
            catch (SocketException)
            {
                LogUtils.AppendLogWarn(LogUtils.ServerConnectionLog, "El servidor fué cerrado mientras esperaba clientes.");
            }
            finally
            {
                listenSocket?.Close();
            }
            LogUtils.AppendLogText(LogUtils.ServerConnectionLog, $"El servidor ya no acepta más clientes ({Computer.N_connections}/{Computer.Max_connections})");
        }

        protected override void Init()
        {
            IsConnected = true;

            audio = new Audio();
            audio.InitializeRecorder();
            audio.CapturedAudio += Audio_CapturedAudio;
            audio.StartRecorder();

            CaptureScreen = new Thread(CaptureScreenThread);
            CaptureScreen.IsBackground = true;
            CaptureScreen.Start();

            CheckCursorShape = new Thread(CheckCursorShapeThread);
            CheckCursorShape.IsBackground = true;
            CheckCursorShape.Start();
        }

        public override void Stop()
        {
            LogUtils.AppendLogText(LogUtils.ServerConnectionLog, "Desconectando servidor...");
            Client[] collection = new Client[clientSockets.Count];
            clientSockets.Values.CopyTo(collection, 0);

            listenSocket?.Close();
            listenThread?.Abort();

            IsConnected = false;

            audio?.Dispose();
            CaptureScreen?.Abort();
            CheckCursorShape?.Abort();

            foreach (Client sockets in collection)
            {
                try
                {
                    LogUtils.AppendLogText(LogUtils.ServerConnectionLog, $"Desconectando al cliente {(sockets.tcpSocket.RemoteEndPoint as IPEndPoint).Address.ToString()}...");
                    DisconnectClient(sockets);
                }
                catch
                {

                }
            }

            Computer.N_connections = 0;
            Computer.Status = 0;
            DBConnection.UpdateComputerInfo(Computer);

            LogUtils.AppendLogFooter(LogUtils.ServerConnectionLog);
        }

        private void DisconnectClient(Client clientSockets)
        {
            lock (clientSockets.udpSocket)
            {
                clientSockets.tcpSocket?.Close();
                clientSockets.udpSocket?.Close();
                clientSockets.mouse?.ReleaseAllClicks();
                clientSockets.keyboard?.ReleaseAllKeys();
            }
        }

        public void CloseConnection(IPAddress IP)
        {
            LogUtils.AppendLogText(LogUtils.ServerConnectionLog, $"Desconectando al cliente {IP}...");
            DisconnectClient(clientSockets[IP]);
            clientSockets.Remove(IP);

            Computer.N_connections--;

            if (Computer.N_connections == 0)
            {
                LogUtils.AppendLogText(LogUtils.ServerConnectionLog, "Se han desconectado todos los clientes, reiniciando el servidor...");
                Stop();
                Start(user);
            }
            else if (Computer.N_connections == Computer.Max_connections - 1)
            {
                LogUtils.AppendLogText(LogUtils.ServerConnectionLog, "El servidor vuelve a tener un espacio libre, volviendo a pedir clientes...");
                Start(user);
            }
            else
                DBConnection.UpdateComputerInfo(Computer);
        }

        public void SendData(byte[] data)
        {
            try
            {
                foreach (Client sockets in clientSockets.Values)
                    SendBuffer(sockets.udpSocket, data);
            }
            catch (InvalidOperationException)
            {
                LogUtils.AppendLogError(LogUtils.ServerConnectionLog, $"La desconexión de un cliente ha provocado que se pierda un paquede ({data[0]}).");
            }
        }

        public void SendWaveFormat(Socket newClientSocket, byte[] waveFormat)
        {
            byte[] petition = new byte[waveFormat.Length + 1];
            petition[0] = Convert.ToByte(Petition.SetWaveFormat);
            Array.Copy(waveFormat, 0, petition, 1, waveFormat.Length);
            SendBuffer(newClientSocket, petition);
        }

        private void Audio_CapturedAudio(object sender, Audio.AudioCapturedEventArgs e)
        {
            SendData(e.Sample);
        }

        private void CaptureScreenThread()
        {
            byte imageCount = 1;
            bool endConnection = false;
            while (!endConnection)
            {
                Task.Run(() =>
                {
                    try
                    {
                        byte[] imageBytes = Screen.CaptureScreen(), info = new byte[5];
                        int dataleft = imageBytes.Length, offset = 0, packetSize;
                        lock (this)
                        {
                            info[0] = (byte)(imageCount);
                            imageCount = (byte)(imageCount % CacheImages + 1);
                        }
                        BitConverter.GetBytes(dataleft).CopyTo(info, 1);
                        SendData(info);

                        while (dataleft > 0)
                        {
                            byte[] rtdpPacket;
                           packetSize = dataleft + 1 > MaxPacketSize ? MaxPacketSize : dataleft + 1;
                           
                            rtdpPacket = new byte[packetSize];                            
                            rtdpPacket[0] = info[0];
                            Array.Copy(imageBytes, offset, rtdpPacket, 1, packetSize - 1);

                            SendData(rtdpPacket);

                            dataleft -= packetSize - 1;
                            offset += packetSize - 1;
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        Console.WriteLine("CaptureScreen, conexión destruida.");
                        endConnection = true;
                    }
                });

                Thread.Sleep(frameRate);
            }
        }

        private void CheckCursorShapeThread()
        {
            IntPtr currentCursor;
            NativeMethods.GetCursorInfo(out CURSORINFO pci);

            currentCursor = pci.hCursor;

            try
            {
                while (IsConnected)
                {
                    pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
                    NativeMethods.GetCursorInfo(out pci);

                    if (currentCursor != pci.hCursor && (int)pci.hCursor <= 0x1001F)
                    {
                        SendData(new byte[] { 0xFF, (byte)pci.hCursor });
                        currentCursor = pci.hCursor;
                    }

                    Thread.Sleep(frameRate);
                }
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("CursorThread, conexión destruida.");
            }
        }

        protected override void ReceivePetition(Socket s, byte[] buffer)
        {
            IPAddress IP = (s.RemoteEndPoint as IPEndPoint).Address;
            Petition petition = (Petition)buffer[0];
            switch (petition)
            {
                case Petition.MouseMove:
                    float xPos = BitConverter.ToSingle(buffer, 1);
                    float yPos = BitConverter.ToSingle(buffer, 5);
                    clientSockets[IP].mouse?.Move(Convert.ToInt32(Screen.Display.Width * xPos / 100), Convert.ToInt32(Screen.Display.Height * yPos / 100));
                    break;
                case Petition.MouseLButtonDown:
                case Petition.MouseRButtonDown:
                case Petition.MouseMButtonDown:
                case Petition.MouseLButtonUp:
                case Petition.MouseRButtonUp:
                case Petition.MouseMButtonUp:
                    clientSockets[IP].mouse?.Button(petition);
                    break;
                case Petition.MouseWheel:
                    clientSockets[IP].mouse?.Wheel(BitConverter.ToInt32(buffer, 1));
                    break;
                case Petition.KeyboardKeyDown:
                case Petition.KeyboardKeyUp:
                    clientSockets[IP].keyboard?.SendKey(buffer[1], petition);
                    break;
                default:
                    throw new Exception();
            }
        }

        private class Client
        {
            public MouseSimulator mouse;
            public DXKeyboardSimulator keyboard;
            public Socket tcpSocket;
            public Socket udpSocket;
        }
    }
}
