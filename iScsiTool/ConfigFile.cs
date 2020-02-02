using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace IScsiTool
{
    class ConfigFile
    {
        public ConfigFile(string path)
        {
            XDocument xDoc = XDocument.Load(path);

            Options = (from xe in xDoc.Descendants("options")
                       select new IScsiOptions(xe)
                      ).FirstOrDefault() ?? new IScsiOptions(null);

            Networks = (from xe in xDoc.Descendants("network")
                        select new IScsiNetwork(xe)
                       ).ToArray();

            Servers = (from xe in xDoc.Descendants("server")
                       select new IScsiServer(xe)
                      ).ToArray();
        }

        public IScsiOptions Options { get; private set; }
        public IScsiNetwork[] Networks { get; private set; }
        public IScsiServer[] Servers { get; private set; }
    }

    class IScsiOptions
    {
        internal IScsiOptions(XElement x)
        {
            QuickTargetPortals = false;
            SetSourcePorts = false;

            string sOp = ReadOptionValue(x, "quick-target-portals");
            if (!string.IsNullOrWhiteSpace(sOp))
                QuickTargetPortals = bool.Parse(sOp);

            sOp = ReadOptionValue(x, "set-source-ports");
            if (!string.IsNullOrWhiteSpace(sOp))
                SetSourcePorts = bool.Parse(sOp);
        }

        private string ReadOptionValue(XElement x, string optionName)
        {
            XElement xOp = x.Element(optionName);
            if (xOp == null) return null;

            XAttribute xVal = xOp.Attribute("value");
            if (xVal == null) return null;

            return xVal.Value;
        }

        public bool QuickTargetPortals { get; private set; }
        public bool SetSourcePorts { get; private set; }
    }

    class IScsiNetwork
    {
        internal IScsiNetwork(XElement x)
        {
            // network prefix
            Prefix = x.Attribute("prefix").Value;

            // local ip on network
            LocalSourceIp = (x.Attribute("source-ip") != null)
                ? IPAddress.Parse(x.Attribute("source-ip").Value) 
                : NetworkHelper.FindLocalIP(Prefix);

            // iSCSI port on network via WMI
            if (LocalSourceIp != null)
                InitiatorPort = WmiHelper.GetIScsiPort(LocalSourceIp);
        }

        public string Prefix { get; private set; }

        public IPAddress LocalSourceIp { get; private set; }

        public uint? InitiatorPort { get; private set; }

        public bool IsValid
        {
            get { return (LocalSourceIp != null); }
        }
    }

    class IScsiServer
    {
        internal IScsiServer(XElement x)
        {
            Fqdn = x.Attribute("fqdn").Value;
            LocalAddr = byte.Parse(x.Attribute("local-addr").Value);

            Targets = (from xe in x.Descendants("target")
                       select new IScsiTarget(xe)
                       ).ToArray();
        }

        public string Fqdn { get; private set; }
        public byte LocalAddr { get; private set; }

        public IPAddress GetNetworkAddress(IScsiNetwork net)
        {
            return IPAddress.Parse(net.Prefix + LocalAddr);
        }

        public IScsiTarget[] Targets { get; private set; }
    }

    class IScsiTarget
    {
        internal IScsiTarget(XElement x)
        {
            Iqn = x.Attribute("iqn").Value;
            
            Count = (x.Attribute("count") != null) 
                ? int.Parse(x.Attribute("count").Value)
                : 1;
        }

        public string Iqn { get; private set; }
        public int Count { get; private set; }
    }
}
