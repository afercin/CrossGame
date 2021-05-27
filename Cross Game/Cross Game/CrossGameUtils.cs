using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Media;

namespace Cross_Game
{
    public enum PressedButton
    {
        Close,
        Maximize,
        Minimize
    }

    public delegate void ClickEventHandler(object sender, ClickEventArgs e);
    public class ClickEventArgs : EventArgs
    {
        public PressedButton PressedButton { get; set; }
        public ClickEventArgs(PressedButton pressedButton) : base() => PressedButton = pressedButton;
    }

    [Serializable]
    public class InternetConnectionException : Exception
    {
        public InternetConnectionException(string msg) : base(msg)
        {
        }
    }

    public class CrossGameUtils
    {
        public static readonly Brush BlackBrush = new SolidColorBrush(Color.FromRgb(50, 50, 50));
        public static readonly Brush GrayBrush = new SolidColorBrush(Color.FromRgb(140, 140, 140));
        public static readonly Brush LightGrayBrush = new SolidColorBrush(Color.FromRgb(179, 179, 179));
        public static readonly Brush WhiteBrush = new SolidColorBrush(Color.FromRgb(230, 230, 230));
        public static readonly Brush RedBrush = new SolidColorBrush(Color.FromRgb(240, 30, 30));
        public static readonly Brush BlueBrush = new SolidColorBrush(Color.FromRgb(50, 157, 201));
        public static readonly Brush YellowBrush = new SolidColorBrush(Color.FromRgb(190, 170, 40));
        public static readonly Brush GreenBrush = new SolidColorBrush(Colors.LimeGreen);

        public static PingReply LastPingResult = null;
        public static bool Ping(string IP)
        {
            try
            {
                LastPingResult = new Ping().Send(IP);
                return LastPingResult.Status == IPStatus.Success;
            }
            catch
            {
                LastPingResult = null;
                return false;
            }
        }

        public static bool HasInternetConnection() => Ping("8.8.8.8");
        
        public static void GetComputerNetworkInfo(out string localIP, out string publicIP, out string mac)
        {
            if (!HasInternetConnection())
                throw new InternetConnectionException("No se tiene acceso a internet");

            localIP = ((IPEndPoint)new UdpClient("8.8.8.8", 1).Client.LocalEndPoint).Address.ToString();
            publicIP = GetPublicIPAddress();
            mac = GetMacByIP(localIP);
        }

        public static string GetPublicIPAddress()
        {
            string address;
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            using (WebResponse response = request.GetResponse())
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                address = stream.ReadToEnd();
            }

            int first = address.IndexOf("Address: ") + 9;
            int last = address.LastIndexOf("</body>");

            return address.Substring(first, last - first);
        }

        private static string GetMacByIP(string ipAddress)
        {
            var query = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n =>
                    n.OperationalStatus == OperationalStatus.Up &&
                    n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(_ => new
                {
                    PhysicalAddress = _.GetPhysicalAddress(),
                    IPProperties = _.GetIPProperties(),
                });

            var mac = query
                .Where(q => q.IPProperties.UnicastAddresses
                    .Any(ua => ua.Address.ToString() == ipAddress))
                .FirstOrDefault()
                .PhysicalAddress;

            return string.Join("-", mac.GetAddressBytes().Select(b => b.ToString("X2")));
        }
               
        public static string GetWindowDiskSN(string logFile)
        {
            try
            {
                string windowsDriveLetter = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)).Split('\\')[0];
                using (var partitions = new ManagementObjectSearcher("ASSOCIATORS OF {Win32_LogicalDisk.DeviceID='" + windowsDriveLetter + "'} WHERE ResultClass=Win32_DiskPartition"))
                    foreach (var partition in partitions.Get())
                        using (var drives = new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + partition["DeviceID"] + "'} WHERE ResultClass=Win32_DiskDrive"))
                            foreach (var drive in drives.Get())
                                return (string)drive["SerialNumber"];
            }
            catch
            {
                LogUtils.AppendLogError(logFile, "404");
                using (var disks = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive"))
                    foreach (var disk in disks.Get())
                        return disk["SerialNumber"].ToString();
            }

            return "<unknown>";
        }
    }
}
