using System;
using System.Net;
using System.Net.Sockets;

namespace RTDP
{
    /* 
     * Dimensiones: 0% --> 100% (simple precisión)
     * 
     *             8 bits                                  32 bits                                                32 bits
     * ╔════════════════════════════╦══════════════════════════════════════════════════════╦══════════════════════════════════════════════════════╗
     * ║          Petition          ║                     xPosition                        ║                     yPosition                        ║ = 9 bytes
     * ╚════════════════════════════╩══════════════════════════════════════════════════════╩══════════════════════════════════════════════════════╝
     * 
     * 
     * Teclas de un teclado: 105 ==> 128 = 2^7 = 7 bits
     * 
     *             8 bits                       8 bits
     * ╔════════════════════════════╦═══════════════════════════╗
     * ║          Petition          ║            keyP           ║ = 2 bytes
     * ╚════════════════════════════╩═══════════════════════════╝
     *
     *
     * Otros eventos:
     * 
     *             8 bits
     * ╔════════════════════════════╗
     * ║          Petition          ║ = 1 byte
     * ╚════════════════════════════╝
     */

    public enum Petition
    {
        MouseMove = 0x01,
        MouseLButtonUp = 0x02,
        MouseLButtonDown = 0x03,
        MouseRButtonUp = 0x04,
        MouseRButtonDown = 0x05,
        MouseMButtonUp = 0x06,
        MouseMButtonDown = 0x07,
        MouseWheel = 0x08,
        KeyboardKeyUp = 0x09,
        KeyboardKeyDown = 0x0A,
        CursorChanged = 0x0B,

        EmulatorInfo = 0x80,
        GameInfo = 0x81,

        ConnectionAccepted = 0xCA,
        ConnectionRefused = EndConnetion,

        SetWaveFormat = 0xFB,
        DecreaseTimeRate = 0xFC,
        IncreaseTimeRate = 0xFD,
        QuitGame = 0xFE,
        EndConnetion = 0xFF,
    }

    public enum CursorShape
    {
        None = 0x00,
        Arrow = 0x03,
        IBeam = 0x05,
        ArrowCD = 0x07,
        Cross = 0x15,
        SizeNWSE = 0x0D,
        SizeNESW = 0x0F,
        SizeWE = 0x11,
        SizeNS = 0x13,
        Wait = 0x19,
        Hand = 0x1F,
        //InvertedArrow = 0x8B,
        //ItalicIBean = 2D,
        //ArrowCross = 4D
    }

    public abstract class RTDProtocol : IDisposable
    {
        public bool IsConnected { get; protected set; }
        public const int MaxPacketSize = 65507;

        protected const int CacheImages = 4;
        protected Audio audio;
        private byte[] key;
        private byte[] bufferCache;

        protected RTDProtocol(string password)
        {
            IsConnected = false;
            key = Crypto.GetBytes(Crypto.CreateSHA256(password));
            Array.Resize(ref key, 32);
            bufferCache = new byte[MaxPacketSize];
        }

        protected void SendBuffer(Socket s, byte[] buffer)
        {
            if (buffer.Length > MaxPacketSize)
                throw new ArgumentException("El tamaño del dato introducido supera el valor de " + MaxPacketSize + " bytes.");
            lock (s)
                s.Send(buffer, buffer.Length, 0);
        }

        protected void ReceiveBuffer(Socket s, out byte[] buffer, out int bufferSize)
        {
            bufferSize = s.Receive(bufferCache, MaxPacketSize, 0);
            buffer = bufferCache;
        }

        protected void SendPetition(Socket s, byte[] buffer)
        {
            SendBuffer(s, Crypto.Encrypt(buffer, buffer.Length, key));
        }

        protected void ReceivePetition(Socket s)
        {
            IPAddress IP = (s.RemoteEndPoint as IPEndPoint).Address;
            int errors = 0;
            try
            {
                while (IsConnected)
                {
                    ReceiveBuffer(s, out byte[] buffer, out int bufferSize);

                    buffer = Crypto.Decrypt(buffer, bufferSize, key);

                    if (buffer[0] == 0)
                    {
                        errors++;
                        if (errors == 5)
                            if (this is RTDPClient)
                            {
                                LogUtils.AppendLogError(LogUtils.ClientConnectionLog, $"Se ha entrado en un bucle de fallos, reiniciando el cliente.");
                                (this as RTDPClient).Restart(true);
                            }
                            else
                            {
                                LogUtils.AppendLogWarn(LogUtils.ServerConnectionLog, $"Parece que la aplicación del cliente {IP} ha crasheado.");
                                (this as RTDPServer).CloseConnection(IP);
                            }
                    }
                    else
                    {
                        errors = 0;
                        ReceivePetition(s, buffer);
                    }
                }
            }
            catch (SocketException e)
            {
                if (IsConnected)
                {
                    if (this is RTDPClient)
                    {
                        LogUtils.AppendLogWarn(LogUtils.ClientConnectionLog, $"Se ha perdido la conexión con el servidor ({e.SocketErrorCode}).");
                        (this as RTDPClient).Restart(true);
                    }
                    else
                    {
                        LogUtils.AppendLogWarn(LogUtils.ServerConnectionLog, $"Se ha perdido la conexión con el cliente {IP} ({e.SocketErrorCode}).");
                        (this as RTDPServer).CloseConnection(IP);
                    }
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }

        protected abstract void ReceivePetition(Socket s, byte[] buffer);
        protected abstract void Init();
        public abstract void Stop();
    }
}
