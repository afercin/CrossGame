using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Cross_Game.Connection
{
    class RTDPClient : RTDProtocol
    {
        public event ReceivedBufferEventHandler ReceivedData;

        private readonly IPEndPoint clientEP;
        private readonly Thread receivePetitionThread;
        private readonly Thread receiveDataThread;
        private readonly Socket petitionsSocket;
        private readonly Socket dataSocket;

        public RTDPClient(int tcpPort, int udpPort, string serverIP) : base()
        {
            clientEP = new IPEndPoint(IPAddress.Any, udpPort);
            serverEP = new IPEndPoint(IPAddress.Parse(serverIP), tcpPort);

            petitionsSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            receivePetitionThread = new Thread(() => ReceivePetition(petitionsSocket));
            receivePetitionThread.IsBackground = true;

            receiveDataThread = new Thread(ReceiveData);
            receiveDataThread.IsBackground = true;
        }

        public override void Start()
        {
            int err = 0;
            bool connected = false;

            Thread connectedThread = new Thread(() =>
            {
                LogUtils.AppendLogHeader(LogUtils.ClientConnectionLog);
                LogUtils.AppendLogText(LogUtils.ClientConnectionLog, $"Intentando conectar con {serverEP.Address.ToString()}:{serverEP.Port}");
                while (!connected && err < 5)
                {
                    LogUtils.AppendLogText(LogUtils.ClientConnectionLog, $"Intento de conexión {err + 1} de 5...");
                    try
                    {
                        petitionsSocket.Connect(serverEP);
                        connected = true;
                    }
                    catch (SocketException e)
                    {
                        LogUtils.AppendLogWarn(LogUtils.ClientConnectionLog, $"No se ha logrado establecer la conexión ({e.SocketErrorCode}).");
                        if (e.SocketErrorCode == SocketError.TimedOut)
                            err = 5;
                        err++;
                    }
                }
            });
            connectedThread.IsBackground = true;

            connectedThread.Start();
            //connectedThread.Join();

            if (!connectedThread.Join(5000))
                if (err == 0)
                {
                    LogUtils.AppendLogError(LogUtils.ClientConnectionLog, "No se ha logrado establecer conexión (TimedOut).");
                    connectedThread.Abort();
                }
                else
                    connectedThread.Join();

            if (connected)
            {
                LogUtils.AppendLogText(LogUtils.ClientConnectionLog, "Mandando credenciales al equipo servidor.");

                /****** Mandar credenciales y esperar confirmación *******/

                byte[] credentials = new byte[] { 0 }, buffer = new byte[] { 0 };
                SendBuffer(petitionsSocket, credentials);
                connectedThread = new Thread(()=>
                {
                    try
                    {
                        ReceiveBuffer(petitionsSocket, out buffer, out int bufferSize);
                    }
                    catch (SocketException)
                    {
                        LogUtils.AppendLogError(LogUtils.ClientConnectionLog, "Se ha cerrado el socket antes de recibir una respuesta del servidor.");
                    }
                });
                connectedThread.IsBackground = true;

                connectedThread.Start();
                if (!connectedThread.Join(5000))
                {
                    LogUtils.AppendLogError(LogUtils.ClientConnectionLog, "No se ha obtenido la respuesta del servidor (TimedOut).");
                    connectedThread.Abort();
                }

                /*********************************************************/

                if (((Petition) buffer[0]) == Petition.ConnectionAccepted)
                {
                    LogUtils.AppendLogText(LogUtils.ClientConnectionLog, "Conexión establecida con éxito.");
                    IsConnected = true;

                    dataSocket.Bind(clientEP);

                    receivePetitionThread.Start();
                    receiveDataThread.Start();

                    LogUtils.AppendLogText(LogUtils.ClientConnectionLog, "Conexión de datos establecida, fin de la configuración.");
                }
                else
                {
                    if (buffer[0] != 0)
                        LogUtils.AppendLogError(LogUtils.ClientConnectionLog, "La conexión ha sido rechazada por el servidor.");
                    petitionsSocket.Close();
                }
            }
            else if (err == 5)
                LogUtils.AppendLogError(LogUtils.ClientConnectionLog, "El servidor no responde.");

            LogUtils.AppendLogFooter(LogUtils.ClientConnectionLog);
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
                    ReceivedData.Invoke(dataSocket, new ReceivedBufferEventArgs(data, dataSize));
                }
            }
            catch (SocketException e)
            {
                LogUtils.AppendLogHeader(LogUtils.ClientConnectionLog);
                LogUtils.AppendLogWarn(LogUtils.ClientConnectionLog, e.Message + $" ({e.SocketErrorCode})");
                LogUtils.AppendLogFooter(LogUtils.ClientConnectionLog);
            }
        }

        public void SendPetition(byte[] petition) => SendBuffer(petitionsSocket, petition);

        public override void Close()
        {
            if (IsConnected)
                SendBuffer(petitionsSocket, new byte[] { Convert.ToByte(Petition.EndConnetion) });
            petitionsSocket.Close();
            dataSocket.Close();
            IsConnected = false;
        }
    }
}
