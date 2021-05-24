using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace RTDP
{   
    [Serializable]
    public class InternetConnectionException : Exception
    {
        public InternetConnectionException(string msg) : base(msg)
        {
        }
    }

    class ConnectionUtils
    {
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
    }
}
