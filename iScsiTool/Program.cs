using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace IScsiTool
{
    class Program
    {
        public const short iSCSI_Port = 3260;
        public const string iSCSI_Initiator = @"ROOT\ISCSIPRT\0000_0";
        public const string iSCSI_MPIO = "0x00000002";

        private static void Main(string[] args)
        {
            if (args.Length < 1)
                args = new[] {"'/?"};

            switch (args[0].ToLowerInvariant())
            {
                case "/config":                    
                    DoConfigure(args);
                    return;
                case "/clear":
                    Cleaner.ClearAll();
                    return;
                default:
                    Console.WriteLine();
                    Console.WriteLine("To Configure iSCSI:");
                    Console.WriteLine(" iScsiTool.exe /config [<xml-config-file> (default: `iscsi.xml`)] [/run]");
                    Console.WriteLine();
                    Console.WriteLine("To Clear iSCSI Configuration:");
                    Console.WriteLine(" iScsiTool.exe /clear");
                    Console.WriteLine();
                    return;    
            }
        }

        private static void DoConfigure(string[] args)
        {
            // validate arguments
            string configFilePath = "iscsi.xml";
            bool runScript = false;
            if (args.Length == 3)
            {
                configFilePath = args[1];

                if (args[2].ToLowerInvariant() == "/run")
                    runScript = true;
            }
            else if (args.Length == 2)
            {
                if (args[1].ToLowerInvariant() == "/run")
                    runScript = true;
                else
                    configFilePath = args[1];
            }

            // load configuration file
            var config = new ConfigFile(configFilePath);

            // output
            using (var swOut = new StreamWriter("iscsi_setup.cmd", false, Encoding.ASCII))
            {                
                foreach (IScsiServer server in config.Servers)
                {
                    swOut.WriteLine("REM - {0}", server.Fqdn);

                    foreach (IScsiNetwork network in config.Networks)
                    {
                        if (config.Options.QuickTargetPortals)
                        {
                            // add target portal command
                            swOut.WriteLine(
                                "IScsiCli.exe QAddTargetPortal {0}", 
                                server.GetNetworkAddress(network)
                                );
                        }
                        else
                        {
                            // check for local source-port
                            string sPort = (config.Options.SetSourcePorts && network.InitiatorPort.HasValue)
                                               ? network.InitiatorPort.ToString() : "*";

                            // add target portal command
                            swOut.WriteLine(
                                "IScsiCli.exe AddTargetPortal {0} {1} {2} {3} * {4} * * * * * * * *",
                                server.GetNetworkAddress(network),
                                iSCSI_Port,
                                iSCSI_Initiator,
                                sPort,
                                iSCSI_MPIO);                            
                        }
                    }
                    swOut.WriteLine();

                    foreach (IScsiTarget target in server.Targets)
                    {
                        swOut.WriteLine("REM - - {0}  ({1})", target.Iqn, target.Count);

                        foreach (IScsiNetwork network in config.Networks)
                        {
                            // check for local source-port
                            string sPort = (config.Options.SetSourcePorts && network.InitiatorPort.HasValue)
                                               ? network.InitiatorPort.ToString() : "*";

                            for (int c = 0; c < target.Count; c++)
                            {
                                // create favorite target entry
                                swOut.WriteLine(
                                    "IScsiCli.exe PersistentLoginTarget {0} T {1} {2} {3} {4} * {5} * * * * * * * * * 0",
                                    target.Iqn,
                                    server.GetNetworkAddress(network),
                                    iSCSI_Port,
                                    iSCSI_Initiator,
                                    sPort,
                                    iSCSI_MPIO);

                                // connect to target
                                swOut.WriteLine(
                                    "IScsiCli.exe LoginTarget {0} T {1} {2} {3} {4} * {5} * * * * * * * * * 0",
                                    target.Iqn,
                                    server.GetNetworkAddress(network),
                                    iSCSI_Port,
                                    iSCSI_Initiator,
                                    sPort,
                                    iSCSI_MPIO);
                            }
                        }
                        swOut.WriteLine();
                    }
                    swOut.WriteLine();
                }
            }

            if (runScript)
                Process.Start("iscsi_setup.cmd");
        }
        
    }
}
