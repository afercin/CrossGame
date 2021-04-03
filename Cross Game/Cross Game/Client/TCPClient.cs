using System;
using System.Net;

namespace Cross_Game.Client
{
    public class TCPClient : TCPSocket
    {
        private readonly IPEndPoint IP;

        public TCPClient(string IP, int port) : base()
        {
            this.IP = new IPEndPoint(IPAddress.Parse(IP), port);
        }

        public override bool Start()
        {
            try
            {
                tcpSocket.Connect(IP);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void SendData(byte[] data) => SendData(data, tcpSocket);

        public byte[] ReceiveData() => ReceiveData(tcpSocket);

        public override void Close()
        {
            SendData(BitConverter.GetBytes((int)Actions.EndConnetion));
        }
    }
}
