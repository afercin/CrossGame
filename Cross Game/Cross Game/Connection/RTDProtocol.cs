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

    abstract class RTDProtocol
    {
        public bool IsConnected { get; protected set; }
        public event ReceivedBufferEventHandler ReceivedPetition;

        public const int MaxPacketSize = 65507;
        protected IPEndPoint serverEP;

        protected RTDProtocol()
        {
            IsConnected = false;
        }

        protected void SendBuffer(Socket s, byte[] buffer)
        {
            if (buffer.Length > MaxPacketSize)
                throw new ArgumentException("El tamaño del dato introducido supera el valor de " + MaxPacketSize + " bytes.");            
            try
            {
                lock (s)
                    s.Send(buffer, buffer.Length, 0);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.Interrupted) // El host remoto ha cerrado la conexión
                    if (this is RTDPServer)
                        (this as RTDPServer).Close(((IPEndPoint)s.RemoteEndPoint).Address.ToString());
                    else
                        Close();
            }
            catch (ObjectDisposedException)
            {
                // Se ha hecho Socket.Close()
            }
        }

        protected void ReceiveBuffer(Socket s, out byte[] buffer, out int bufferSize)
        {
            buffer = new byte[MaxPacketSize];
            bufferSize = s.Receive(buffer, MaxPacketSize, 0);
        }

        public abstract void Start();
        public abstract void Close();

        protected void ReceivePetition(Socket s)
        {
            try
            {
                while (IsConnected)
                {
                    ReceiveBuffer(s, out byte[] buffer, out int bufferSize);
                    ReceivedPetition.Invoke(s, new ReceivedBufferEventArgs(buffer, bufferSize));
                }
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.Interrupted) // El host remoto ha cerrado la conexión
                    if (this is RTDPServer)
                        (this as RTDPServer).Close(((IPEndPoint)s.RemoteEndPoint).Address.ToString());
                    else
                        Close();
            }
            catch (ObjectDisposedException)
            {
                // Se ha hecho Socket.Close()
            }
        }
    }
}
