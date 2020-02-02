using System.Net;
using System.Net.NetworkInformation;

namespace IScsiTool
{
    static class NetworkHelper
    {
        public static IPAddress FindLocalIP(string startsWith)
        {
            // find local source IP on network
            foreach (NetworkInterface netIf in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (netIf.OperationalStatus != OperationalStatus.Up)
                    continue;

                IPInterfaceProperties ipProps = netIf.GetIPProperties();
                if (ipProps == null) continue;

                foreach (UnicastIPAddressInformation addr in ipProps.UnicastAddresses)
                {
                    if (addr.Address == null) continue;
                    if (addr.Address.ToString().StartsWith(startsWith))
                        return addr.Address;
                }
            }

            return null;
        }
    }
}
