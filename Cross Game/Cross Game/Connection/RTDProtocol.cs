using Cross_Game.DataManipulation;
using NAudio.Wave;
using System;
using System.Net;
using System.Net.Sockets;

namespace Cross_Game.Connection
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
        MouseMove = 0x00,
        MouseLButtonUp = 0x01,
        MouseLButtonDown = 0x02,
        MouseRButtonUp = 0x03,
        MouseRButtonDown = 0x04,
        MouseMButtonUp = 0x05,
        MouseMButtonDown = 0x06,
        MouseWheel = 0x07,
        KeyboardKeyUp = 0x08,
        KeyboardKeyDown = 0x09,
        CursorChanged = 0x0A,

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
        Hand = 0x1F
    }

    abstract class RTDProtocol
    {
        public bool IsConnected { get; protected set; }

        public const int MaxPacketSize = 65507;
        protected IPEndPoint serverEP;
        protected Audio audio;

        protected RTDProtocol()
        {
            IsConnected = false;
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
            buffer = new byte[MaxPacketSize];
            bufferSize = s.Receive(buffer, MaxPacketSize, 0);
        }

        protected void ReceivePetition(Socket s)
        {            
            try
            {
                while (IsConnected)
                {
                    ReceiveBuffer(s, out byte[] buffer, out int bufferSize);
                    ReceivePetition(buffer);
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
                        string IP = (s.RemoteEndPoint as IPEndPoint).Address.ToString();
                        LogUtils.AppendLogWarn(LogUtils.ServerConnectionLog, $"Se ha perdido la conexión con el cliente {IP} ({e.SocketErrorCode}).");
                        (this as RTDPServer).CloseConnection(IP);
                    }
                }
            }
        }

        protected abstract void ReceivePetition(byte[] buffer);
        protected abstract void Init();
        public abstract void Start();
        public abstract void Stop();
    }
}
