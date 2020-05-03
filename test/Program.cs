using System;
using System.Collections.Generic;
using System.Security;
using System.Threading;
using AssetDiscovery;
using AssetDiscovery.Premise;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Windows Credentials
            SecureString securepassword = new SecureString();
            string plaintextpassword = "admin@2009";
            foreach (char c in plaintextpassword)
            {
                securepassword.AppendChar(c);
            }
            IMachineCredentials mcwin = new WindowsCredentials()
            {
                Domain = string.Empty,
                Username = "administrator",
                Password = securepassword
            };
            #endregion

            #region UNIX Credentials
            securepassword = new SecureString();
            plaintextpassword = "pass@123";
            foreach (char c in plaintextpassword)
            {
                securepassword.AppendChar(c);
            }
            IMachineCredentials mcunix = new UNIXCredentials
            {
                Username = "arsim",
                Password = securepassword
            };
            #endregion

            #region Network Device Credentials
            securepassword = new SecureString();
            plaintextpassword = "cisco";
            foreach (char c in plaintextpassword)
            {
                securepassword.AppendChar(c);
            }
            SecureString ensecurepassword = new SecureString();
            string enplaintextpassword = "cisco";
            foreach (char c in enplaintextpassword)
            {
                ensecurepassword.AppendChar(c);
            }
            IMachineCredentials mcnwdev = new NetworkDeviceCredentials
            {
                Username = "cisco",
                Password = securepassword,
                ENPassword = ensecurepassword
            };
            #endregion

            List<Scripts> scrlst = new List<Scripts>();
            scrlst.Add(
                new Scripts(){
                    Script = ScriptType.POWERSHELL,
                    ScriptName = "IISVersion",
                    ScriptText = "(Get-ItemProperty -Path HKLM:\\SOFTWARE\\Microsoft\\InetStp).versionString"
                }
            );
            scrlst.Add(
                new Scripts(){
                    Script = ScriptType.BASH,
                    ScriptName = "PERLVERSION",
                    ScriptText = "perl --version | head -n 2"
                }
            );

            LocalPremise localPremise = new LocalPremise();
            localPremise.WindowsCredentials = mcwin;
            localPremise.UNIXCredentials = mcunix;
            localPremise.NetworkDeviceCredentials = mcnwdev;
            localPremise.Scan("10.10.5.40/29");
            while (!localPremise.IsComplete)
            {
                Thread.Sleep(10000);
            }
            List<MachineInfo> minfos = localPremise.ServerFound;
            foreach (MachineInfo mi in minfos)
            {
                Console.WriteLine("IPAddress: " + mi.IPAddress);
                Console.WriteLine("ispingable: " + mi.IsPingable);
                Console.WriteLine("OSType: " + mi.OSType);
                if (!string.IsNullOrEmpty(mi.OSVersion))
                    Console.WriteLine("OSVersion: " + mi.OSVersion);
                if (!string.IsNullOrEmpty(mi.Hostname))
                    Console.WriteLine("Hostname: " + mi.Hostname);
                foreach (KeyValuePair<string, uint> entry in mi.OpenedPorts)
                {
                    Console.WriteLine(entry.Key + " : " + entry.Value);
                }
                foreach (KeyValuePair<string, string> entry in mi.ScriptOutput)
                {
                    Console.WriteLine(entry.Key + " : " + entry.Value);
                }
                Console.WriteLine(Environment.NewLine);
            }
        }
    }
}
