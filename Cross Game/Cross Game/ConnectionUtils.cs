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
        public static byte[] Compress(byte[] data)
        {
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        public static byte[] Decompress(byte[] data)
        {
            MemoryStream input = new MemoryStream(data);
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }

        public static bool InternetConnection() => new Ping().Send("8.8.8.8").Status == IPStatus.Success;

        public static string GetPublicIPAddress()
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

        public static string GetLocalIPAddress()
        {
            if (!InternetConnection())
                throw new InternetConnectionException("You has not internet connection");
            return ((IPEndPoint)new UdpClient("8.8.8.8", 1).Client.LocalEndPoint).Address.ToString();
        }

        public static string GetMacByIP(string ipAddress)
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
