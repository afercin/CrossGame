using System.Net;
using System.Net.Sockets;

namespace Cross_Game.Server
{
    class TCPServer : TCPSocket
    {
        public Socket clientSocket;

        public IPEndPoint ClientIP
        {
            get => clientSocket.RemoteEndPoint as IPEndPoint;
        }

        public TCPServer(int port) : base() => tcpSocket.Bind(new IPEndPoint(IPAddress.Any, port));

        public void SendData(byte[] data) => SendData(data, clientSocket);

        public byte[] ReceiveData() => ReceiveData(clientSocket);

        public override bool Start()
        {
            tcpSocket.Listen(1);
            try
            {
                clientSocket = tcpSocket.Accept();
                return true;
            }
            catch
            {
                return false; // Servidor cerrado antes de que pudiese encontrar clientes.
            }
        }

        public override void Close()
        {
            clientSocket?.Close();
            tcpSocket.Close();
        }
    }
}
