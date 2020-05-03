using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using AssetDiscovery.Communicators;
using LukeSkywalker.IPNetwork;

namespace AssetDiscovery.Premise
{
    public class LocalPremise : IPremise
    {
        private WindowsCredentials windowsCredentials;
        private UNIXCredentials uNIXCredentials;
        private NetworkDeviceCredentials networkDeviceCredentials;
        private LukeSkywalker.IPNetwork.IPAddressCollection ipac = null;
        static readonly object _locker = new object();
        private IPAddress firstusable = null;
        private IPAddress lastusable = null;
        private uint usablenumber = 0;
        private uint sshport = 22;
        private uint winrmport = 5986;
        private uint rdpport = 3389;
        private uint telnetport = 23;
        private bool iscomplete = false;
        private List<MachineInfo> serversfound = new List<MachineInfo>();
        private List<Scripts> scrlst = new List<Scripts>();


        public List<Scripts> ScriptList
        {
            set
            {
                scrlst = value;
            }
        }
        public string Name => "LOCAL";
        public bool IsComplete => iscomplete;
        public PremiseType Premise => PremiseType.LOCAL;

        public event MachineInfoOutput NotifyMachineInfo;

        public IMachineCredentials WindowsCredentials
        {
            set { windowsCredentials = (WindowsCredentials)value; }
        }

        public IMachineCredentials UNIXCredentials
        {
            set { uNIXCredentials = (UNIXCredentials)value; }
        }

        public IMachineCredentials NetworkDeviceCredentials
        {
            set { networkDeviceCredentials = (NetworkDeviceCredentials)value; }
        }

        public uint SSH
        {
            get { return sshport; }
            set { sshport = value; }
        }
        public uint WINRM
        {
            get { return winrmport; }
            set { winrmport = value; }
        }
        public uint RDP
        {
            get { return rdpport; }
            set { rdpport = value; }
        }
        public uint TELNET
        {
            get { return telnetport; }
            set { telnetport = value; }
        }

        public List<MachineInfo> ServerFound
        {
            get { return serversfound; }
        }

        public void Scan(string iprange)
        {
            IPNetwork ipnetwork = IPNetwork.Parse(iprange);
            firstusable = ipnetwork.FirstUsable;
            lastusable = ipnetwork.LastUsable;
            usablenumber = ipnetwork.Usable;
            ipac = IPNetwork.ListIPAddress(ipnetwork);
            StartScan();
        }

        public void StartScan()
        {
            bool touse = false;
            NotifyMachineInfo += new MachineInfoOutput(AddMachineInfo);
            foreach (IPAddress ipaddr in ipac)
            {
                if (ipaddr.Equals(firstusable))
                    touse = true;
                if (touse)
                    Task.Factory.StartNew((x) => GetOSTypeParallel((IPAddress)x), ipaddr);
                if (ipaddr.Equals(lastusable))
                    touse = false;
            }
        }

        private void GetOSTypeParallel(IPAddress ipaddr)
        {
            // Console.WriteLine("Started for " + ipaddr);
            MachineInfo mi = GetOSType(ipaddr);
            // Console.WriteLine("Ended for " + ipaddr);
            NotifyMachineInfo?.Invoke(mi);
        }

        private MachineInfo GetOSType(IPAddress ipaddress)
        {
            // First try to PING
            bool ispingable = false;
            bool isssh = false;
            bool isrdp = false;
            bool iswinrm = false;
            bool istelnet = false;

            MachineInfo minfo = new MachineInfo()
            {
                IPAddress = ipaddress,
                OpenedPorts = new Dictionary<string, uint>(),
                ScriptOutput = new Dictionary<string, string>(),
                Hostname = string.Empty,
                OSVersion = string.Empty
            };

            using (Ping p = new Ping())
            {
                PingReply reply = p.Send(ipaddress);
                if (reply.Status == IPStatus.Success)
                {
                    ispingable = true;
                }
            }
            minfo.IsPingable = ispingable;
            // Second try SSH
            ICommunication sshconn = new SSHCommunication()
            {
                Port = sshport,
                IPAddress = ipaddress,
                TargetCredentials = uNIXCredentials
            };
            isssh = sshconn.IsConnectSuccessfull();
            if (isssh)
            {
                minfo.OpenedPorts.Add("SSH", sshport);
            }
            // third try RDP
            ICommunication rdpconn = new RDPCommunication()
            {
                Port = rdpport,
                IPAddress = ipaddress,
                TargetCredentials = windowsCredentials
            };
            isrdp = rdpconn.IsConnectSuccessfull();
            if (isrdp)
            {
                minfo.OpenedPorts.Add("RDP", rdpport);
            }
            // fourth try WINRM
            ICommunication winrmconn = new WinRMCommunication()
            {
                Port = winrmport,
                IPAddress = ipaddress,
                TargetCredentials = windowsCredentials,
                CheckCert = false,
                IsSSL = true
            };
            iswinrm = winrmconn.IsConnectSuccessfull();
            if (iswinrm)
            {
                minfo.OpenedPorts.Add("WINRM", winrmport);
            }
            // last try TELNET
            ICommunication telnetconn = new TelnetCommunication()
            {
                Port = telnetport,
                IPAddress = ipaddress,
                TargetCredentials = networkDeviceCredentials
            };
            istelnet = telnetconn.IsConnectSuccessfull();
            if (istelnet)
            {
                minfo.OpenedPorts.Add("TELNET", telnetport);
            }
            //Get OS Version
            //Get Hostname
            if (isrdp || iswinrm)
            {
                //ip is windows
                minfo.OSType = "WINDOWS";
                // code for using multiple protocol like winrm HTTP, WMI etc.
                #region Get Hostname and OSversion using WINRM HTTPS
                if (iswinrm)
                {
                    minfo.Hostname = winrmconn.GetHostname();
                    minfo.OSVersion = winrmconn.GetOSVersion();
                    foreach (Scripts scr in scrlst)
                    {
                        if (scr.Script == ScriptType.POWERSHELL)
                        {
                            string result = winrmconn.RunCommand(scr.ScriptText);
                            minfo.ScriptOutput.Add(scr.ScriptName, result);
                        }
                    }
                }
                #endregion
            }
            else if (isssh)
            {
                //ip is UNIX or Network Device
                minfo.Hostname = sshconn.GetHostname();
                if (string.IsNullOrEmpty(minfo.Hostname))
                {
                    minfo.OSType = "UNIXORNETWORKDEVICE";
                }
                else
                {
                    minfo.OSType = "UNIX";
                    minfo.OSVersion = sshconn.GetOSVersion();
                    foreach (Scripts scr in scrlst)
                    {
                        if (scr.Script == ScriptType.BASH)
                        {
                            string result = sshconn.RunCommand(scr.ScriptText);
                            minfo.ScriptOutput.Add(scr.ScriptName, result);
                        }
                    }
                }
            }
            else if (istelnet)
            {
                //ip is network device
                minfo.OSType = "NETWORKDEVICE";
            }
            if (!(isrdp || iswinrm || isssh || istelnet))
            {
                if (ispingable)
                {
                    //ip exists but cannot connect in any known way
                    minfo.OSType = "UNKNOWN";
                }
                else
                {
                    //ip does not exists
                    minfo.OSType = "DOESNOTEXIST";
                }
            }
            return minfo;
        }

        private void AddMachineInfo(MachineInfo minfo)
        {
            if (minfo.OSType != "DOESNOTEXIST")
            {
                serversfound.Add(minfo);
            }
            lock (_locker)
            {
                usablenumber--;
            }
            if (usablenumber == 0)
                iscomplete = true;
        }
    }
}
