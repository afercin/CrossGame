using Cross_Game.DataManipulation;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Cross_Game.Connection
{
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
    class RTDPController : IDisposable
    {
        public event CursorShangedEventHandler CursorShapeChanged;
        public event ImageBuiltEventHandler ImageBuilt;

        public bool IsConnected { get => connection.IsConnected; }

        private readonly Dictionary<int, ScreenImage> images;

        private Thread CaptureScreen;
        private Thread CheckCursorShape;
        private int timeRate;

        private RTDProtocol connection;
        private Audio audio;

        private RTDPController()
        {
            audio = null;
        }

        public RTDPController(int tcpPort, int udpPort, string serverIP) : this()
        {
            RTDPClient client = new RTDPClient(tcpPort, udpPort, serverIP);
            client.ReceivedPetition += Link_ReceivedPetition;
            client.ReceivedData += RTDPClient_ReceivedData;

            images = new Dictionary<int, ScreenImage>();

            connection = client;
            client.Start();
        }

        public RTDPController(int tcpPort, int udpPort, int maxConnections, int fps) : this()
        {
            RTDPServer server = new RTDPServer(tcpPort, udpPort);
            server.ReceivedPetition += Link_ReceivedPetition;
            server.ClientConnected += RTDPServer_ClientConnected;
            server.MaxConnections = maxConnections;

            timeRate = 1000 / fps;

            connection = server;
            server.Start();
        }

        private void RTDPServer_ClientConnected(object sender, EventArgs e)
        {
            if (audio == null)
            {
                audio = new Audio();
                audio.InitializeRecorder();
                audio.CapturedAudio += Audio_CapturedAudio;
                audio.StartRecorder();
                CaptureScreen = new Thread(CaptureScreenThread);
                CaptureScreen.IsBackground = true;
                CaptureScreen.Start();
                CheckCursorShape = new Thread(CheckCursorShapeThread);
                CheckCursorShape.IsBackground = true;
                CheckCursorShape.Start();
            }

            (connection as RTDPServer).SendWaveFormat(sender as Socket, new byte[] { 0 }); // TODO: Enviar waveformat
        }

        private void Audio_CapturedAudio(object sender, Audio.AudioCapturedEventArgs e) => (connection as RTDPServer).SendData(e.Sample);

        private void CaptureScreenThread()
        {
            RTDPServer server = connection as RTDPServer;
            byte imageCount = 1;
            while (audio != null)
            {
                Task.Run(() =>
                {
                    byte[] imageBytes = Screen.CaptureScreen(), info = new byte[5];
                    int dataleft = imageBytes.Length, offset = 0, packetSize;
                    lock (server)
                    {
                        info[0] = imageCount;
                        imageCount = (byte) (imageCount % 254 + 1);
                    }
                    BitConverter.GetBytes(dataleft).CopyTo(info, 1);
                    server.SendData(info);

                    while (dataleft > 0)
                    {
                        byte[] rtdpPacket;
                        packetSize = dataleft + 1 > RTDProtocol.MaxPacketSize ? RTDProtocol.MaxPacketSize : dataleft + 1;

                        rtdpPacket = new byte[packetSize];
                        rtdpPacket[0] = info[0];
                        Array.Copy(imageBytes, offset, rtdpPacket, 1, packetSize - 1);

                        server.SendData(rtdpPacket);

                        dataleft -= packetSize - 1;
                        offset += packetSize - 1;
                    }
                });

                Thread.Sleep(timeRate);
            }
        }

        private void CheckCursorShapeThread()
        {
            IntPtr currentCursor;
            User32.GetCursorInfo(out User32.CURSORINFO pci);

            currentCursor = pci.hCursor;

            while (audio != null)
            {
                pci.cbSize = Marshal.SizeOf(typeof(User32.CURSORINFO));
                User32.GetCursorInfo(out pci);

                if (currentCursor != pci.hCursor && (int)pci.hCursor <= 0x1001F)
                {
                    (connection as RTDPServer).SendData(new byte[] { 0xFF, (byte)pci.hCursor });
                    currentCursor = pci.hCursor;
                }

                Thread.Sleep(timeRate);
            }
        }

        private void RTDPClient_ReceivedData(object sender, ReceivedBufferEventArgs e)
        {
            if (e.Buffer[0] == 0x00) // Nuevo audio
            {
                audio?.PlayAudio(e.Buffer);
            }   
            else if(e.Buffer[0] < 0xFF) // Nueva imagen
            {
                if (e.BufferSize == 5) // Se empieza a transmitir una nueva imagen
                {
                    images[e.Buffer[0]] = new ScreenImage(BitConverter.ToInt32(e.Buffer, 1));
                }
                else // Agregar buffer a la imagen correspondiente
                {
                    int img = e.Buffer[0];
                    try
                    {
                        if (images[img].AppendBuffer(e.Buffer, 1, e.BufferSize - 1))
                            ImageBuilt.Invoke(this, new ImageBuiltEventArgs(images[img].ImageBytes));
                    }
                    catch (KeyNotFoundException)
                    {
                        Console.WriteLine("No ha llegado a tiempo el paquete que inicializaba el fotograma nº" + img);
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine("No ha llegado a tiempo el paquete que inicializaba el fotograma nº" + img);
                    }
                }
            }
            else // Nueva forma del cursor
            {
                CursorShapeChanged.Invoke(this, new CursorShangedEventArgs((CursorShape)e.Buffer[1]));
            }
        }

        private void Link_ReceivedPetition(object sender, ReceivedBufferEventArgs e)
        {
            Petition petition = (Petition)e.Buffer[0];
            switch (petition)
            {
                case Petition.EndConnetion:
                    if (connection is RTDPClient)
                        connection.Close();
                    else
                    {
                        RTDPServer server = connection as RTDPServer;
                        server.Close(((IPEndPoint)(sender as Socket).RemoteEndPoint).Address.ToString());
                        if (server.NClients == 0)
                        {
                            audio.CapturedAudio -= Audio_CapturedAudio;
                            audio.Dispose();
                            audio = null;
                        }
                    }
                    break;
                case Petition.IncreaseTimeRate: timeRate += e.Buffer[1]; break;
                case Petition.DecreaseTimeRate: timeRate -= e.Buffer[1]; break;
                case Petition.MouseMove:                    
                    float xPos = BitConverter.ToSingle(e.Buffer, 1);
                    float yPos = BitConverter.ToSingle(e.Buffer, 5);
                    MouseSimulator.Move(Convert.ToInt32(Screen.Display.Width * xPos / 100), Convert.ToInt32(Screen.Display.Height * yPos / 100));
                    break;
                case Petition.MouseLButtonDown:
                case Petition.MouseRButtonDown:
                case Petition.MouseMButtonDown:
                case Petition.MouseLButtonUp:
                case Petition.MouseRButtonUp:
                case Petition.MouseMButtonUp:
                    MouseSimulator.Button(petition);
                    break;
                case Petition.MouseWheel:
                    MouseSimulator.Wheel(BitConverter.ToInt32(e.Buffer, 1));
                    break;
                case Petition.KeyboardKeyDown:
                case Petition.KeyboardKeyUp:
                    DXKeyboardSimulator.SendKey(e.Buffer[1], petition);
                    break;
                case Petition.SetWaveFormat:
                    WasapiLoopbackCapture cosa = new WasapiLoopbackCapture();
                    audio = new Audio();
                    audio.StartPlayer(cosa.WaveFormat);
                    break;
            }
        }

        public void SendMousePosition(Point position, Size RenderSize)
        {
            byte[] petition = new byte[9];

            petition[0] = Convert.ToByte(Petition.MouseMove);
            BitConverter.GetBytes(Convert.ToSingle(position.X * 100.0 / RenderSize.Width)).CopyTo(petition, 1);
            BitConverter.GetBytes(Convert.ToSingle(position.Y * 100.0 / RenderSize.Height)).CopyTo(petition, 5);

            (connection as RTDPClient).SendPetition(petition);
        }

        public void SendMouseButton(MouseButton mouseButton, bool isPressed)
        {
            switch (mouseButton)
            {
                case MouseButton.Left: SendOtherEvents(isPressed ? Petition.MouseLButtonDown : Petition.MouseLButtonUp); break;
                case MouseButton.Right: SendOtherEvents(isPressed ? Petition.MouseRButtonDown : Petition.MouseRButtonUp); break;
                case MouseButton.Middle: SendOtherEvents(isPressed ? Petition.MouseMButtonDown : Petition.MouseMButtonUp); break;
            }
        }

        public void SendMouseScroll(int delta)
        {
            byte[] petition = new byte[5];

            petition[0] = Convert.ToByte(Petition.MouseWheel);
            BitConverter.GetBytes(delta).CopyTo(petition, 1);

            (connection as RTDPClient).SendPetition(petition);
        }

        public void SendKey(Key key, bool isPressed)
        {
            byte[] petition = new byte[2];

            petition[0] = Convert.ToByte(isPressed ? Petition.KeyboardKeyDown : Petition.KeyboardKeyUp);
            petition[1] = Convert.ToByte(key);

            (connection as RTDPClient).SendPetition(petition);
        }

        public void SendOtherEvents(Petition petition) => (connection as RTDPClient).SendPetition(new byte[] { Convert.ToByte(petition) });

        public void Dispose()
        {
            audio?.Dispose();
            connection.Close();
        }

        private class ScreenImage
        {
            public byte[] ImageBytes { get; set; }

            private int imageSize;
            private int currentSize;

            public ScreenImage(int size)
            {
                ImageBytes = new byte[size];
                imageSize = size;
                currentSize = 0;
            }

            public bool AppendBuffer(byte[] buffer, int offset, int bufferSize)
            {
                Array.Copy(buffer, offset, ImageBytes, currentSize, bufferSize);
                currentSize += bufferSize;

                return currentSize == imageSize;
            }
        }

        private class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int x;
                public int y;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct CURSORINFO
            {
                public int cbSize;        // Specifies the size, in bytes, of the structure. 
                                          // The caller must set this to Marshal.SizeOf(typeof(CURSORINFO)).
                public int flags;         // Specifies the cursor state. This parameter can be one of the following values:
                                          //    0             The cursor is hidden.
                                          //    CURSOR_SHOWING    The cursor is showing.
                public IntPtr hCursor;          // Handle to the cursor. 
                public POINT ptScreenPos;       // A POINT structure that receives the screen coordinates of the cursor. 
            }

            [DllImport("user32.dll")]
            public static extern bool GetCursorInfo(out CURSORINFO pci);
        }
    }
}
