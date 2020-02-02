using System;
using System.Management;
using System.Net;

namespace IScsiTool
{
    static class WmiHelper
    {
        public static uint? GetIScsiPort(IPAddress srcIp)
        {
            try
            {
                var searcher = new ManagementObjectSearcher("root\\WMI", "SELECT PortalInformation FROM MSiSCSI_PortalInfoClass");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    var portalArray = (ManagementBaseObject[])queryObj["PortalInformation"];
                    foreach (ManagementBaseObject portalObj in portalArray)
                    {
                        var ipObj = (ManagementBaseObject)portalObj["IpAddr"];
                        var ipv4Num = (uint)ipObj["IpV4Address"];
                        if (ipv4Num > 0)
                        {
                            var ipAddr = new IPAddress(ipv4Num);
                            if (ipAddr.Equals(srcIp))
                                return (uint)portalObj["Port"];
                        }
                    }
                }
            }
            catch (ManagementException e)
            {
                Console.WriteLine("An error occurred while querying for WMI data: " + e.Message);
            }

            return null;
        }
    }
}
