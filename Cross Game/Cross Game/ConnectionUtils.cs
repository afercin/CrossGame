using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Cross_Game
{   
    class ConnectionUtils
    {
        public static bool Ping(string IP) => new Ping().Send(IP).Status == IPStatus.Success;

        public static bool InternetConnection() => Ping("8.8.8.8");
        
        public static void GetComputerNetworkInfo(out string localIP, out string publicIP, out string mac)
        {
            localIP = GetLocalIPAddress();
            publicIP = GetPublicIPAddress();
            mac = GetMacByIP(localIP);
        }

        private static string GetPublicIPAddress()
        {
            if (!InternetConnection())
                throw new InternetConnectionException("You has not internet connection");
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

        private static string GetLocalIPAddress()
        {
            if (!InternetConnection())
                throw new InternetConnectionException("You has not internet connection");
            return ((IPEndPoint)new UdpClient("8.8.8.8", 1).Client.LocalEndPoint).Address.ToString();
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
