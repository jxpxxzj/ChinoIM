using System;
using System.Net;
using System.Net.Sockets;

namespace ChinoIM.Common.Helpers
{
    public class NetworkUtil
    {
        private static bool checkAddressFamilyAvailable(AddressFamily family)
        {
            var allIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (var ip in allIPs)
            {
                if (ip.AddressFamily == family)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsSupportIPv6 => checkAddressFamilyAvailable(AddressFamily.InterNetworkV6);

        public static bool IsSupportIPv4 => checkAddressFamilyAvailable(AddressFamily.InterNetwork);

        public static TcpClient GetSuitableTcpClient(AddressFamily force = AddressFamily.Unspecified)
        {
            if (force != AddressFamily.Unspecified)
            {
                return new TcpClient(force);
            }

            if (IsSupportIPv6)
            {
                return new TcpClient(AddressFamily.InterNetworkV6);
            }

            if (IsSupportIPv4)
            {
                return new TcpClient(AddressFamily.InterNetwork);
            }

            throw new Exception("No suitable IP protocol.");
        }
    }
}
