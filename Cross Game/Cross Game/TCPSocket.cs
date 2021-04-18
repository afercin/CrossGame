using System;
using System.Net.Sockets;

namespace Cross_Game
{
    public abstract class TCPSocket
    {
        protected Socket tcpSocket;

        public TCPSocket() => tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        protected void SendData(byte[] data, Socket s)
        {
            if (data.Length > ConnectionUtils.MaxPacketSize)
                throw new ArgumentException("El tamaño del dato introducido supera el valor de " + ConnectionUtils.MaxPacketSize + " bytes.");
            try
            {
                lock (s)
                {
                    s.Send(BitConverter.GetBytes(data.Length));
                    s.Send(data);
                }
            }
            catch { }
        }

        protected byte[] ReceiveData(Socket s)
        {
            byte[] datasize = new byte[4], data;
            int size;

            s.Receive(datasize, 0, 4, 0);
            size = BitConverter.ToInt32(datasize, 0);

            if (size <= 0)
                return null;

            data = new byte[size];
            s.Receive(data, 0, data.Length, 0);

            return data;
        }
        public abstract void Close();
    }
}
