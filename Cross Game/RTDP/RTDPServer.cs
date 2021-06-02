using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace RTDP
{
    public class RTDPServer : RTDProtocol
    {
        public event EventHandler ConnectedClientsChanged;
        public event GotClientCredentialsEventHandler GotClientCredentials;

        private Dictionary<IPAddress, Client> clientSockets;
        private Thread listenThread;
        private Thread CaptureScreen;
        private Thread CheckCursorShape;
        private Socket listenSocket;
        private int frameRate;

        public RTDPServer(string password) : base(password)
        {

        }

        public void Start(ref ComputerData currentComputer)
        {
            clientSockets = new Dictionary<IPAddress, Client>();

            computer = currentComputer;
            computer.Status = 1;
            frameRate = 1000 / computer.FPS;
            
            listenThread = new Thread(ConnectionThread);
            listenThread.IsBackground = true;
            listenThread.Start();
        }

        private void ConnectionThread()
        {
            Socket tcpClientSocket;
            Socket udpClientSocket;
            IPEndPoint clientAddress;
            Thread receivePetitionThread;
            byte[] buffer = null;
            int bufferSize = 0;
            
            LogUtils.AppendLogText(LogUtils.ServerConnectionLog, $"Comenzando a recibir clientes por el puerto {computer.Tcp}");

            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(IPAddress.Any, computer.Tcp));
            listenSocket.Listen(computer.Max_connections - computer.N_connections);
            try
            {
                while (computer.N_connections < computer.Max_connections)
                {
                    tcpClientSocket = listenSocket.Accept();
                    clientAddress = tcpClientSocket.RemoteEndPoint as IPEndPoint;
                    LogUtils.AppendLogText(LogUtils.ServerConnectionLog, $"Intento de conexión del cliente con IP {clientAddress.Address.ToString()}");

                    receivePetitionThread = new Thread(() => ReceiveBuffer(tcpClientSocket, out buffer, out bufferSize));
                    receivePetitionThread.IsBackground = true;
                    receivePetitionThread.Start();

                    if (receivePetitionThread.Join(10000))
                        try
                        {
                            buffer = Crypto.Decrypt(buffer, bufferSize, key);

                            var credentials = new GotClientCredentialsEventArgs(Crypto.GetString(buffer, buffer.Length).Replace("\0", "").Split(';'));
                            GotClientCredentials.Invoke(this, credentials);

                            if (credentials.UserPriority > 0)
                            {
                                LogUtils.AppendLogOk(LogUtils.ServerConnectionLog, $"Cliente {clientAddress.Address.ToString()} aceptado, procediendo a establecer canal de comunicación.");

                                SendBuffer(tcpClientSocket, new byte[] { Convert.ToByte(Petition.ConnectionAccepted) });

                                if (computer.N_connections == 0)
                                    Init();

                                byte[] waveformat = new byte[] { 0 };
                                SendWaveFormat(tcpClientSocket, waveformat);
                                udpClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                                udpClientSocket.Connect(new IPEndPoint(clientAddress.Address, computer.Udp));

                                Client client = new Client()
                                {
                                    mouse = credentials.UserPriority == 2 ? new MouseSimulator() : null,
                                    keyboard = new DXKeyboardSimulator(),
                                    tcpSocket = tcpClientSocket,
                                    udpSocket = udpClientSocket
                                };


                                clientSockets[clientAddress.Address] = client;

                                receivePetitionThread = new Thread(() => ReceivePetition(tcpClientSocket));
                                receivePetitionThread.IsBackground = true;
                                receivePetitionThread.Start();

                                computer.N_connections++;

                                ConnectedClientsChanged.Invoke(this, new EventArgs());

                                LogUtils.AppendLogOk(LogUtils.ServerConnectionLog, $"Cliente con IP {clientAddress.Address.ToString()} agregado con éxito.");
                            }
                            else
                            {
                                SendBuffer(tcpClientSocket, new byte[] { Convert.ToByte(Petition.ConnectionRefused) });
                                LogUtils.AppendLogWarn(LogUtils.ServerConnectionLog, $"No se acepta la conexión desde {clientAddress.Address.ToString()} (usuario no autorizado).");
                            }
                        }
                        catch
                        {
                            SendBuffer(tcpClientSocket, new byte[] { Convert.ToByte(Petition.ConnectionRefused) });
                            LogUtils.AppendLogWarn(LogUtils.ServerConnectionLog, $"No se acepta la conexión desde {clientAddress.Address.ToString()} (no ha sido posible desencriptar el mensaje).");
                        }
                    else
                    {
                        receivePetitionThread.Abort();
                        LogUtils.AppendLogWarn(LogUtils.ServerConnectionLog, $"No se acepta la conexión desde {clientAddress.Address.ToString()} (TimeOut).");
                    }
                }
            }
            catch (ThreadAbortException)
            {

            }
            catch (SocketException)
            {
                LogUtils.AppendLogWarn(LogUtils.ServerConnectionLog, "El servidor fué cerrado mientras esperaba clientes.");
            }
            finally
            {
                listenSocket?.Close();
            }
            LogUtils.AppendLogText(LogUtils.ServerConnectionLog, $"El servidor ya no acepta más clientes ({computer.N_connections}/{computer.Max_connections})");
        }

        protected override void Init()
        {
            IsConnected = true;

            audio = new Audio();
            audio.InitializeRecorder();
            audio.CapturedAudio += Audio_CapturedAudio;
            
            CaptureScreen = new Thread(CaptureScreenThread);
            CaptureScreen.IsBackground = true;
            CaptureScreen.Start();

            CheckCursorShape = new Thread(CheckCursorShapeThread);
            CheckCursorShape.IsBackground = true;
            CheckCursorShape.Start();
        }

        public override void Stop()
        {
            LogUtils.AppendLogText(LogUtils.ServerConnectionLog, "Desconectando servidor...");
            Client[] collection = new Client[clientSockets.Count];
            clientSockets.Values.CopyTo(collection, 0);

            listenSocket?.Close();
            listenThread?.Abort();

            IsConnected = false;

            audio?.Dispose();
            CaptureScreen?.Abort();
            CheckCursorShape?.Abort();

            foreach (Client sockets in collection)
            {
                try
                {
                    LogUtils.AppendLogText(LogUtils.ServerConnectionLog, $"Desconectando al cliente {(sockets.tcpSocket.RemoteEndPoint as IPEndPoint).Address.ToString()}...");
                    DisconnectClient(sockets);
                }
                catch
                {

                }
            }

            computer.N_connections = 0;
            computer.Status = 0;
            
            LogUtils.AppendLogFooter(LogUtils.ServerConnectionLog);
        }

        private void DisconnectClient(Client clientSockets)
        {
            lock (clientSockets.udpSocket)
            {
                clientSockets.tcpSocket?.Close();
                clientSockets.udpSocket?.Close();
                clientSockets.mouse?.ReleaseAllClicks();
                clientSockets.keyboard?.ReleaseAllKeys();
            }
        }

        public void CloseConnection(IPAddress IP)
        {
            LogUtils.AppendLogText(LogUtils.ServerConnectionLog, $"Desconectando al cliente {IP}...");
            DisconnectClient(clientSockets[IP]);
            clientSockets.Remove(IP);

            computer.N_connections--;

            if (computer.N_connections == 0)
            {
                LogUtils.AppendLogText(LogUtils.ServerConnectionLog, "Se han desconectado todos los clientes, reiniciando el servidor...");
                Stop();
                Start(ref computer);
            }
            else if (computer.N_connections == computer.Max_connections - 1)
            {
                LogUtils.AppendLogText(LogUtils.ServerConnectionLog, "El servidor vuelve a tener un espacio libre, volviendo a pedir clientes...");
                Start(ref computer);
            }

            ConnectedClientsChanged.Invoke(this, new EventArgs());
        }

        public void SendData(byte[] data)
        {
            try
            {
                foreach (Client sockets in clientSockets.Values)
                    SendBuffer(sockets.udpSocket, data);
            }
            catch (InvalidOperationException)
            {
                LogUtils.AppendLogError(LogUtils.ServerConnectionLog, $"La desconexión de un cliente ha provocado que se pierda un paquede ({data[0]}).");
            }
        }

        public void SendWaveFormat(Socket newClientSocket, byte[] waveFormat)
        {
            byte[] petition = new byte[waveFormat.Length + 1];
            petition[0] = Convert.ToByte(Petition.SetWaveFormat);
            Array.Copy(waveFormat, 0, petition, 1, waveFormat.Length);
            SendPetition(newClientSocket, petition);
        }

        private void Audio_CapturedAudio(object sender, AudioCapturedEventArgs e)
        {
            SendData(e.Sample);
        }

        private void InitCaptureScreen(Adapter1 adapter, SharpDX.Direct3D11.Device device, out Bitmap bitmap, out Texture2D screenTexture, out OutputDuplication duplicatedOutput)
        {
            var output = adapter.GetOutput(0);
            var output1 = output.QueryInterface<Output1>();

            Display.Width = output.Description.DesktopBounds.Right;
            Display.Height = output.Description.DesktopBounds.Bottom;

            var textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = Display.Width,
                Height = Display.Height,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            };

            bitmap = new Bitmap(Display.Width, Display.Height, PixelFormat.Format32bppRgb);
            screenTexture = new Texture2D(device, textureDesc);
            duplicatedOutput = output1.DuplicateOutput(device);
        }

        private void CaptureScreenThread()
        {
            byte[] info = new byte[5];
            byte imageCount = 1;
            bool endConnection = false;

            var adapter = new Factory1().GetAdapter1(0);
            var device = new SharpDX.Direct3D11.Device(adapter);

            InitCaptureScreen(adapter, device,
                              out Bitmap bitmap,
                              out Texture2D screenTexture,
                              out OutputDuplication duplicatedOutput);

            Texture2D screenTexture2D;
            DataBox mapSource;
            Rectangle boundsRect;
            BitmapData mapDest;
            IntPtr sourcePtr, destPtr;
            Bitmap r = new Bitmap(1, 1);
            Stopwatch stopwatch = new Stopwatch();
            try
            {
                while (!endConnection)
                {
                    try
                    {
                        stopwatch.Reset();
                        stopwatch.Start();
                        duplicatedOutput.AcquireNextFrame(1000, out OutputDuplicateFrameInformation duplicateFrameInformation, out SharpDX.DXGI.Resource screenResource);

                        screenTexture2D = screenResource.QueryInterface<Texture2D>();
                        device.ImmediateContext.CopyResource(screenTexture2D, screenTexture);

                        mapSource = device.ImmediateContext.MapSubresource(screenTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
                        boundsRect = new Rectangle(0, 0, Display.Width, Display.Height);

                        mapDest = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                        sourcePtr = mapSource.DataPointer;
                        destPtr = mapDest.Scan0;
                        for (int y = 0; y < Display.Height; y++)
                        {
                            Utilities.CopyMemory(destPtr, sourcePtr, Display.Width * 4);

                            sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                            destPtr = IntPtr.Add(destPtr, mapDest.Stride);
                        }

                        bitmap.UnlockBits(mapDest);
                        device.ImmediateContext.UnmapSubresource(screenTexture, 0);
                        r = new Bitmap(bitmap, 700, 393);
                        Task.Factory.StartNew(() =>
                        {
                            byte[] imageBytes;
                            using (var ms = new MemoryStream())
                            {
                                r.Save(ms, ImageFormat.Jpeg);
                                r.Dispose();
                                imageBytes = ms.ToArray();
                            }
                            int dataleft = imageBytes.Length, offset = 0, packetSize;
                            info[0] = imageCount;
                            imageCount = (byte)(imageCount % CacheImages + 1);
                            BitConverter.GetBytes(dataleft).CopyTo(info, 1);
                            SendData(info);

                            while (dataleft > 0)
                            {
                                byte[] rtdpPacket;
                                packetSize = dataleft + 1 > MaxPacketSize ? MaxPacketSize : dataleft + 1;

                                rtdpPacket = new byte[packetSize];
                                rtdpPacket[0] = info[0];
                                Array.Copy(imageBytes, offset, rtdpPacket, 1, packetSize - 1);

                                SendData(rtdpPacket);

                                dataleft -= packetSize - 1;
                                offset += packetSize - 1;
                            }
                        });
                        screenResource.Dispose();
                        duplicatedOutput?.ReleaseFrame();
                    }
                    catch (SharpDXException e)
                    {
                        if (e.ResultCode.Code == SharpDX.DXGI.ResultCode.AccessLost.Result.Code)
                        {
                            bitmap.Dispose();
                            InitCaptureScreen(adapter, device, out bitmap, out screenTexture, out duplicatedOutput);
                        }
                        else if (e.ResultCode.Code != SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                        {
                            Trace.TraceError(e.Message);
                            Trace.TraceError(e.StackTrace);
                        }
                    }
                    catch (NullReferenceException)
                    {
                        endConnection = true;
                    }
                    catch (ObjectDisposedException)
                    {
                        Console.WriteLine("CaptureScreen, conexión destruida.");
                        endConnection = true;
                    }
                    stopwatch.Stop();
                    Console.WriteLine("Fotograma enviado: {0}ms", stopwatch.ElapsedMilliseconds);
                    Thread.Sleep((int)Math.Max(0, frameRate - 1 - stopwatch.ElapsedMilliseconds));
                }
            }
            catch (ThreadAbortException)
            {

            }
            finally
            {
                duplicatedOutput.Dispose();
                bitmap.Dispose();
            }
        }

        private void CheckCursorShapeThread()
        {
            IntPtr currentCursor;
            NativeMethods.GetCursorInfo(out CURSORINFO pci);

            currentCursor = pci.hCursor;

            try
            {
                while (IsConnected)
                {
                    pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
                    NativeMethods.GetCursorInfo(out pci);

                    if (currentCursor != pci.hCursor && (int)pci.hCursor <= 0x1001F)
                    {
                        SendData(new byte[] { 0xFF, (byte)pci.hCursor });
                        currentCursor = pci.hCursor;
                    }

                    Thread.Sleep(frameRate);
                }
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("CursorThread, conexión destruida.");
            }
        }

        protected override void ReceivePetition(Socket s, byte[] buffer)
        {
            IPAddress IP = (s.RemoteEndPoint as IPEndPoint).Address;
            Petition petition = (Petition)buffer[0];
            switch (petition)
            {
                case Petition.MouseMove:
                    float xPos = BitConverter.ToSingle(buffer, 1);
                    float yPos = BitConverter.ToSingle(buffer, 5);
                    clientSockets[IP].mouse?.Move(Convert.ToInt32(Display.Width * xPos / 100), Convert.ToInt32(Display.Height * yPos / 100));
                    break;
                case Petition.MouseLButtonDown:
                case Petition.MouseRButtonDown:
                case Petition.MouseMButtonDown:
                case Petition.MouseLButtonUp:
                case Petition.MouseRButtonUp:
                case Petition.MouseMButtonUp:
                    clientSockets[IP].mouse?.Button(petition);
                    break;
                case Petition.MouseWheel:
                    clientSockets[IP].mouse?.Wheel(BitConverter.ToInt32(buffer, 1));
                    break;
                case Petition.KeyboardKeyDown:
                case Petition.KeyboardKeyUp:
                    clientSockets[IP].keyboard?.SendKey(buffer[1], petition);
                    break;
            }
        }

        private class Display
        {
            public static int Width { get; set; }
            public static int Height { get; set; }
        }

        private class Client
        {
            public MouseSimulator mouse;
            public DXKeyboardSimulator keyboard;
            public Socket tcpSocket;
            public Socket udpSocket;
        }
    }
}
