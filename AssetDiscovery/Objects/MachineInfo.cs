using System.Collections.Generic;
using System.Net;

namespace AssetDiscovery
{
    public class MachineInfo
    {
        public IPAddress IPAddress { get; set; }
        public string Hostname { get; set; }
        public string OSType { get; set; }
        public string OSVersion { get; set; }
        public bool IsPingable { get; set; }
        public Dictionary<string, uint> OpenedPorts { get; set; }
    }

    public delegate void MachineInfoOutput(MachineInfo minfo);
}