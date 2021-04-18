using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Cross_Game.Connection
{
    class RTDPServer : RTDProtocol
    {

        public int MaxConnections { get; set; }
        public event EventHandler ClientConnected;

        public int NClients { get => clientSockets.Count; }
        private Dictionary<string, Sockets> clientSockets;
        private readonly int udpPort;
        private Thread listenThread;
        private Socket listenSocket;

        public RTDPServer(int tcpPort, int udpPort) : base()
        {
            serverEP = new IPEndPoint(IPAddress.Any, tcpPort);
            clientSockets = new Dictionary<string, Sockets>();
            this.udpPort = udpPort;
        }

        public override void Start()
        {
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
            LogUtils.AppendLogHeader(LogUtils.ServerConnectionLog);
            LogUtils.AppendLogText(LogUtils.ServerConnectionLog, $"Comenzando a recibir clientes por el puerto {serverEP.Port}");

            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(serverEP);
            listenSocket.Listen(MaxConnections);
            try
            {
                while (NClients < MaxConnections) // diseñar alguna forma de recibir clientes permanentemente
                {
                    tcpClientSocket = listenSocket.Accept();
                    clientAddress = tcpClientSocket.RemoteEndPoint as IPEndPoint;
                    LogUtils.AppendLogText(LogUtils.ServerConnectionLog, $"Intento de conexión del cliente con IP {clientAddress.Address.ToString()}");
                    /***** Esperar y comprobar credenciales del usuario *****/

                    ReceiveBuffer(tcpClientSocket, out byte[] buffer, out int bufferSize);

                    /*********************************************************/
                    if (DBConnection.CheckLogin("afercin@gmail.com", "patata123") == 1)
                    {
                        LogUtils.AppendLogText(LogUtils.ServerConnectionLog, $"Cliente {clientAddress.Address.ToString()} aceptado, procediendo a establecer canal de comunicación.");

                        SendBuffer(tcpClientSocket, new byte[] { Convert.ToByte(Petition.ConnectionAccepted) });
                        ClientConnected.Invoke(tcpClientSocket, new EventArgs());
                        IsConnected = true;

                        udpClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        udpClientSocket.Connect(new IPEndPoint(clientAddress.Address, udpPort));

                        clientSockets[clientAddress.Address.ToString()] = new Sockets()
                        {
                            tcpSocket = tcpClientSocket,
                            udpSocket = udpClientSocket
                        };

                        receivePetitionThread = new Thread(() => ReceivePetition(tcpClientSocket));
                        receivePetitionThread.IsBackground = true;
                        receivePetitionThread.Start();
                        LogUtils.AppendLogText(LogUtils.ServerConnectionLog, $"Cliente con IP {clientAddress.Address.ToString()} agregado con éxito.");
                    }
                    else
                    {
                        LogUtils.AppendLogWarn(LogUtils.ServerConnectionLog, $"No se acepta la conexión desde {clientAddress.Address.ToString()}, datos de inicio de sesión incorrectos.");
                        SendBuffer(tcpClientSocket, new byte[] { Convert.ToByte(Petition.ConnectionRefused) });
                    }
                }
            }
            catch (ThreadAbortException)
            {
                LogUtils.AppendLogWarn(LogUtils.ServerConnectionLog, "Se ha forzado el apagado del servidor.");
                listenSocket?.Close();
            }
            catch (Exception e)
            {
                LogUtils.AppendLogError(LogUtils.ServerConnectionLog, e.Message);
                LogUtils.AppendLogError(LogUtils.ServerConnectionLog, e.StackTrace);
            }
            listenSocket.Close();
            LogUtils.AppendLogText(LogUtils.ServerConnectionLog, $"El servidor ya no acepta más clientes ({NClients}/{MaxConnections})");
            LogUtils.AppendLogFooter(LogUtils.ServerConnectionLog);
        }

        public override void Close()
        {
            Sockets[] collection = new Sockets[clientSockets.Count];
            clientSockets.Values.CopyTo(collection, 0);
            foreach (Sockets sockets in collection)
            {
                SendBuffer(sockets.tcpSocket, new byte[] { Convert.ToByte(Petition.EndConnetion) });
                sockets.tcpSocket.Close();
                sockets.udpSocket.Close();
            }
            listenSocket?.Close();
            IsConnected = false;
        }

        public bool Close(string IP)
        {
            clientSockets[IP].tcpSocket?.Close();
            clientSockets[IP].udpSocket?.Close();

            clientSockets.Remove(IP);

            if (NClients == 0)
            {
                IsConnected = false;
                listenThread?.Abort();
                Start();
            }

            return NClients == 0;
        }

        public void SendData(byte[] data)
        {
            try
            {
                foreach (Sockets sockets in clientSockets.Values)
                    SendBuffer(sockets.udpSocket, data);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Se ha desconectado un cliente.");
            }
        }

        public void SendWaveFormat(Socket newClientSocket, byte[] waveFormat)
        {
            byte[] petition = new byte[waveFormat.Length + 1];
            petition[0] = Convert.ToByte(Petition.SetWaveFormat);
            Array.Copy(waveFormat, 0, petition, 1, waveFormat.Length);
            SendBuffer(newClientSocket, petition);
        }

        private class Sockets
        {
            public Socket tcpSocket;
            public Socket udpSocket;
        }
    }
}
