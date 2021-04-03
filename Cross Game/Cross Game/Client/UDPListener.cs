using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Cross_Game.Client
{
    class UDPListener
    {
        private Socket udpListener;
        private Thread listenThread;
        private ImageBuilder imageBuilder;

        public event ImageBuiltEventHandler ImageBuilt;

        public UDPListener(int port)
        {
            udpListener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpListener.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            udpListener.Bind(new IPEndPoint(IPAddress.Any, port));

            imageBuilder = new ImageBuilder();

            listenThread = new Thread(ListenThread);
            listenThread.IsBackground = true;
            listenThread.Start();
        }

        public void Close()
        {
            listenThread?.Abort();
            listenThread = null;

            udpListener?.Close();
            udpListener = null;
        }

        private void ListenThread()
        {
            while (udpListener != null)
            {
                byte[] data = new byte[ConnectionUtils.MaxPacketSize], image;
                int recv = udpListener.Receive(data, ConnectionUtils.MaxPacketSize, 0);

                image = imageBuilder.Build(data, recv);

                if (image != null)
                    ImageBuilt(this, new ImageBuiltEventArgs(image));
            }
        }
    }

    public delegate void ImageBuiltEventHandler(object sender, ImageBuiltEventArgs e);

    public class ImageBuiltEventArgs : EventArgs
    {
        public byte[] imageBytes;

        public ImageBuiltEventArgs(byte[] imageBytes)
        {
            this.imageBytes = imageBytes;
        }
    }
}
