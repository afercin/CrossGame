﻿using Cross_Game.DataManipulation;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Cross_Game.Connection
{
    class RTDPServer : RTDProtocol
    {
        public int TimeRate { get; set; }
        public int MaxConnections { get; set; }

        public int NClients { get => clientSockets.Count; }
        private Dictionary<string, Sockets> clientSockets;
        private readonly int udpPort;
        private Thread listenThread;
        private Thread CaptureScreen;
        private Thread CheckCursorShape;
        private Socket listenSocket;


        public RTDPServer(int tcpPort, int udpPort) : base()
        {
            serverEP = new IPEndPoint(IPAddress.Any, tcpPort);
            this.udpPort = udpPort;
        }

        public override void Start()
        {
            if (!IsConnected || NClients == 0)
                clientSockets = new Dictionary<string, Sockets>();

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
            LogUtils.AppendLogHeader(LogUtils.ServerConnectionLog);
            LogUtils.AppendLogText(LogUtils.ServerConnectionLog, $"Comenzando a recibir clientes por el puerto {serverEP.Port}");

            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(serverEP);
            listenSocket.Listen(MaxConnections - NClients);
            try
            {
                while (NClients < MaxConnections) // diseñar alguna forma de recibir clientes permanentemente
                {
                    tcpClientSocket = listenSocket.Accept();
                    clientAddress = tcpClientSocket.RemoteEndPoint as IPEndPoint;
                    LogUtils.AppendLogText(LogUtils.ServerConnectionLog, $"Intento de conexión del cliente con IP {clientAddress.Address.ToString()}");
                    /***** Esperar y comprobar credenciales del usuario *****/

                    ReceiveBuffer(tcpClientSocket, out byte[] buffer, out int bufferSize);

                    /*********************************************************/
                    if (DBConnection.CheckLogin("afercin@gmail.com", "patata123") == 1)
                    {
                        LogUtils.AppendLogText(LogUtils.ServerConnectionLog, $"Cliente {clientAddress.Address.ToString()} aceptado, procediendo a establecer canal de comunicación.");

                        SendBuffer(tcpClientSocket, new byte[] { Convert.ToByte(Petition.ConnectionAccepted) });

                        if (NClients == 0)
                            Init();

                        SendWaveFormat(tcpClientSocket, new byte[] { 0 }); // TODO: Enviar waveformat

                        udpClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        udpClientSocket.Connect(new IPEndPoint(clientAddress.Address, udpPort));

                        clientSockets[clientAddress.Address.ToString()] = new Sockets()
                        {
                            tcpSocket = tcpClientSocket,
                            udpSocket = udpClientSocket
                        };

                        receivePetitionThread = new Thread(() => ReceivePetition(tcpClientSocket));
                        receivePetitionThread.IsBackground = true;
                        receivePetitionThread.Start();
                        LogUtils.AppendLogText(LogUtils.ServerConnectionLog, $"Cliente con IP {clientAddress.Address.ToString()} agregado con éxito.");
                    }
                    else
                    {
                        LogUtils.AppendLogWarn(LogUtils.ServerConnectionLog, $"No se acepta la conexión desde {clientAddress.Address.ToString()}, datos de inicio de sesión incorrectos.");
                        SendBuffer(tcpClientSocket, new byte[] { Convert.ToByte(Petition.ConnectionRefused) });
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Recojo la excepcion que salta al abortar el thread para que no vaya al log.
            }
            catch (Exception e)
            {
                LogUtils.AppendLogError(LogUtils.ConnectionErrorsLog, e.Message);
                LogUtils.AppendLogError(LogUtils.ConnectionErrorsLog, e.StackTrace);
            }
            finally
            {
                listenSocket?.Close();
            }
            LogUtils.AppendLogText(LogUtils.ServerConnectionLog, $"El servidor ya no acepta más clientes ({NClients}/{MaxConnections})");
        }

        protected override void Init()
        {
            IsConnected = true;

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

        public override void Stop()
        {
            LogUtils.AppendLogText(LogUtils.ServerConnectionLog, "Desconectando servidor...");
            Sockets[] collection = new Sockets[clientSockets.Count];
            clientSockets.Values.CopyTo(collection, 0);

            listenSocket?.Close();
            listenThread?.Abort();

            IsConnected = false;

            audio?.Dispose();
            CaptureScreen?.Abort();
            CheckCursorShape?.Abort();

            foreach (Sockets sockets in collection)
            {
                //if (IsConnected)
                //    SendBuffer(sockets.tcpSocket, new byte[] { Convert.ToByte(Petition.EndConnetion) });
                LogUtils.AppendLogText(LogUtils.ServerConnectionLog, $"Desconectando al cliente {(sockets.tcpSocket.RemoteEndPoint as IPEndPoint).Address.ToString()}...");
                lock (sockets.udpSocket)
                {
                    sockets.tcpSocket.Close();
                    sockets.udpSocket.Close();
                }

            }
            LogUtils.AppendLogFooter(LogUtils.ServerConnectionLog);
        }

        public void CloseConnection(string IP)
        {
            LogUtils.AppendLogText(LogUtils.ServerConnectionLog, $"Desconectando al cliente {IP}...");
            clientSockets[IP].tcpSocket?.Close();
            clientSockets[IP].udpSocket?.Close();
            clientSockets.Remove(IP);

            if (NClients == 0)
            {
                LogUtils.AppendLogText(LogUtils.ServerConnectionLog, "Se han desconectado todos los clientes, reiniciando el servidor...");
                Stop();
                Start();
            }
            else if (NClients == MaxConnections - 1)
            {
                LogUtils.AppendLogText(LogUtils.ServerConnectionLog, "Se han desconectado un cliente, volviendo a pedir clientes...");
                Start();
            }
        }

        public void SendData(byte[] data)
        {
            foreach (Sockets sockets in clientSockets.Values)
            {
                try
                {
                    SendBuffer(sockets.udpSocket, data);
                }
                catch
                {

                }
            }
        }

        public void SendWaveFormat(Socket newClientSocket, byte[] waveFormat)
        {
            byte[] petition = new byte[waveFormat.Length + 1];
            petition[0] = Convert.ToByte(Petition.SetWaveFormat);
            Array.Copy(waveFormat, 0, petition, 1, waveFormat.Length);
            SendBuffer(newClientSocket, petition);
        }

        private void Audio_CapturedAudio(object sender, Audio.AudioCapturedEventArgs e)
        {
            SendData(e.Sample);
        }

        private void CaptureScreenThread()
        {
            byte imageCount = 1;
            bool endConnection = false;
            while (!endConnection)
            {
                Task.Run(() =>
                {
                    try
                    {
                        byte[] imageBytes = Screen.CaptureScreen(), info = new byte[5];
                        int dataleft = imageBytes.Length, offset = 0, packetSize;
                        lock (this)
                        {
                            info[0] = (byte)(imageCount);
                            imageCount = (byte)(imageCount % CacheImages + 1);
                        }
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
                    }
                    catch (ObjectDisposedException)
                    {
                        Console.WriteLine("CaptureScreen, conexión destruida.");
                        endConnection = true;
                    }
                });

                Thread.Sleep(TimeRate);
            }
        }

        private void CheckCursorShapeThread()
        {
            IntPtr currentCursor;
            Win32API.GetCursorInfo(out CURSORINFO pci);

            currentCursor = pci.hCursor;

            try
            {
                while (IsConnected)
                {
                    pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
                    Win32API.GetCursorInfo(out pci);

                    if (currentCursor != pci.hCursor && (int)pci.hCursor <= 0x1001F)
                    {
                        SendData(new byte[] { 0xFF, (byte)pci.hCursor });
                        currentCursor = pci.hCursor;
                    }

                    Thread.Sleep(TimeRate);
                }
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("CursorThread, conexión destruida.");
            }
        }

        protected override void ReceivePetition(byte[] buffer)
        {
            Petition petition = (Petition)buffer[0];
            switch (petition)
            {
                //case Petition.EndConnetion:
                //    if (connection is RTDPClient)
                //        connection.Close();
                //    else
                //    {
                //        RTDPServer server = connection as RTDPServer;
                //        server.Close(((IPEndPoint)(sender as Socket).RemoteEndPoint).Address.ToString());
                //        if (server.NClients == 0)
                //        {
                //            audio.CapturedAudio -= Audio_CapturedAudio;
                //            audio.Dispose();
                //            audio = null;
                //        }
                //    }
                //    break;
                case Petition.IncreaseTimeRate: TimeRate += buffer[1]; break;
                case Petition.DecreaseTimeRate: TimeRate -= buffer[1]; break;
                case Petition.MouseMove:
                    float xPos = BitConverter.ToSingle(buffer, 1);
                    float yPos = BitConverter.ToSingle(buffer, 5);
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
                    MouseSimulator.Wheel(BitConverter.ToInt32(buffer, 1));
                    break;
                case Petition.KeyboardKeyDown:
                case Petition.KeyboardKeyUp:
                    DXKeyboardSimulator.SendKey(buffer[1], petition);
                    break;
                default:
                    throw new Exception();
            }
        }

        private class Sockets
        {
            public Socket tcpSocket;
            public Socket udpSocket;
        }
    }
}
