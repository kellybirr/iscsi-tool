using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace IScsiTool
{
    class Cleaner
    {
        public static void ClearAll()
        {
            ClearConnections();
            Console.WriteLine();

            ClearFavorites();
            Console.WriteLine();

            ClearTargetPortals();
            Console.WriteLine();
        }

        static void ClearConnections()
        {
            var proc = Process.Start(new ProcessStartInfo
            {
                FileName = "IScsiCLi.exe",
                Arguments = "SessionList",
                RedirectStandardOutput = true,
                UseShellExecute = false
            });

            string sData = proc.StandardOutput.ReadToEnd();

            Match match = Regex.Match(sData, @"Session Id\s*\:\s(?<Id>[a-z0-9\-]*)", RegexOptions.Multiline);
            while (match.Success)
            {
                string sId = match.Groups["Id"].Value;
                string procArgs = string.Format("LogoutTarget {0}", sId);

                Console.WriteLine(procArgs);
                Process.Start(new ProcessStartInfo("IScsiCli.exe", procArgs) { WindowStyle = ProcessWindowStyle.Hidden });

                match = match.NextMatch();
            }
        }

        static void ClearFavorites()
        {
            var proc = Process.Start(new ProcessStartInfo
            {
                FileName = "IScsiCLi.exe",
                Arguments = "ListPersistentTargets",
                RedirectStandardOutput = true,
                UseShellExecute = false
            });

            string sData = proc.StandardOutput.ReadToEnd();

            Match match = Regex.Match(sData, @"Target Name\s*\:\s(?<t>[\w\.\-\:]*)\s*\n\s*Address and Socket\s*\:\s(?<a>[\d\.]+ \d+)", RegexOptions.Multiline);
            while (match.Success)
            {
                string initName = Program.iSCSI_Initiator;
                string target = match.Groups["t"].Value;
                string port = "*";
                string addr = match.Groups["a"].Value;

                // get additional args
                string sPart2 = sData.Substring(match.Index, sData.IndexOf("Security Flags", match.Index) - match.Index);

                // check for initiator name
                Match mInit = Regex.Match(sPart2, @"Initiator Name\s*:\s(?<i>[\w\\]*)\s*\n\s*Port", RegexOptions.Multiline);
                if (mInit.Success && (!string.IsNullOrWhiteSpace(mInit.Groups["i"].Value)))
                {
                    initName = mInit.Groups["i"].Value;

                    // specific port?
                    Match mPort = Regex.Match(sPart2, @"Port Number\s*\:\s(?<p>\d+)", RegexOptions.Multiline);
                    if (mPort.Success && (!string.IsNullOrWhiteSpace(mPort.Groups["p"].Value)))
                        port = mPort.Groups["p"].Value;
                }

                string procArgs = string.Format("RemovePersistentTarget {0} {1} {2} {3}", initName, target, port, addr);

                Console.WriteLine(procArgs);
                Process.Start(new ProcessStartInfo("IScsiCli.exe", procArgs) { WindowStyle = ProcessWindowStyle.Hidden });

                match = match.NextMatch();
            }
        }

        private static void ClearTargetPortals()
        {
            var proc = Process.Start(new ProcessStartInfo
            {
                FileName = "IScsiCLi.exe",
                Arguments = "ListTargetPortals",
                RedirectStandardOutput = true,
                UseShellExecute = false
            });

            string sData = proc.StandardOutput.ReadToEnd();

            Match match = Regex.Match(sData, @"Address and Socket\s*\:\s(?<a>[\d\.]+ \d+)", RegexOptions.Multiline);
            while (match.Success)
            {
                // default args
                string addr = match.Groups["a"].Value;
                string procArgs = string.Format("RemoveTargetPortal {0}", addr);

                // get additional args
                string sPart2 = sData.Substring(match.Index, sData.IndexOf("Security Flags", match.Index) - match.Index);

                // check for initiator name
                Match mInit = Regex.Match(sPart2, @"Initiator Name\s*:\s(?<i>[\w\\]*)\s*\n\s*Port", RegexOptions.Multiline);
                if (mInit.Success && (!string.IsNullOrWhiteSpace(mInit.Groups["i"].Value)))
                {
                    procArgs += " " + mInit.Groups["i"].Value;

                    // specific port?
                    Match mPort = Regex.Match(sPart2, @"Port Number\s*\:\s(?<p>\d+)", RegexOptions.Multiline);
                    if (mPort.Success && (!string.IsNullOrWhiteSpace(mPort.Groups["p"].Value)))
                        procArgs += " " + mPort.Groups["p"].Value;
                }

                Console.WriteLine(procArgs);
                Process.Start(new ProcessStartInfo("IScsiCli.exe", procArgs) { WindowStyle = ProcessWindowStyle.Hidden });

                match = match.NextMatch();
            }            
        }
    }
}
