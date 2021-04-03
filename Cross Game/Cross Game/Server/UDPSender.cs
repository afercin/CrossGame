using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Cross_Game.Server
{
    class UDPSender
    {
        private Socket udpSender;
        private Thread udpSenderThread;
        private byte imageCount;
        
        internal class Display
        {
            public static int Width { get; set; }
            public static int Height { get; set; }
            public static int Left { get; set; }
            public static int Top { get; set; }
        }

        public UDPSender(string IP, int port)
        {
            udpSender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSender.Connect(new IPEndPoint(IPAddress.Parse(IP), port));
            
            imageCount = 0;

            udpSenderThread = new Thread(UDPSenderThread);
            udpSenderThread.IsBackground = true;
            udpSenderThread.Start();
        }

        public void Close()
        {
            udpSender?.Close();
            udpSender = null;

            udpSenderThread?.Abort();
            udpSenderThread = null;
        }

        private byte[] ScreenToBytes()
        {
            byte[] data;
            using (Bitmap b = new Bitmap(Display.Width, Display.Height))
            {
                using (Graphics g = Graphics.FromImage(b))
                    g.CopyFromScreen(Display.Left, Display.Top, 0, 0, b.Size);
                using (MemoryStream ms = new MemoryStream())
                {
                    b.Save(ms, ImageFormat.Jpeg);
                    data = ms.ToArray();
                }
            }
            return data;
        }

        private void UDPSenderThread()
        {
            while (udpSender != null)
            {
                Task.Run(() =>
                {
                    try
                    {
                        byte[] image = ScreenToBytes(), info = new byte[5];
                        int dataleft = image.Length, sent, offset = 0;
                        lock (udpSender)
                        {
                            info[0] = imageCount;
                            imageCount++;
                            imageCount %= 60;
                        }
                        BitConverter.GetBytes(dataleft).CopyTo(info, 1);
                        udpSender.Send(info, info.Length, 0);

                        while (dataleft > 0)
                        {
                            int packetLenght = dataleft < ConnectionUtils.MaxPacketSize ? dataleft + 1 : ConnectionUtils.MaxPacketSize;
                            byte[] rtspPacket = new byte[packetLenght];

                            rtspPacket[0] = info[0];
                            Array.Copy(image, offset, rtspPacket, 1, packetLenght - 1);

                            sent = udpSender.Send(rtspPacket, packetLenght, 0);

                            dataleft -= sent - 1;
                            offset += sent - 1;
                        }
                    }
                    catch { }
                });
                Thread.Sleep(ConnectionUtils.Delay);
            }
        }
    }
}
