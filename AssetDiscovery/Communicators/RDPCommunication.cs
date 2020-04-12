using System;
using System.Net;
using System.Net.Sockets;

namespace AssetDiscovery.Communicators
{
    public class RDPCommunication : ICommunication
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private uint port = 3389;
        private IPAddress ip;
        private WindowsCredentials wincred;
        public string Name => "RDP";

        public uint Port
        {
            get { return port; }
            set { port = value; }
        }
        public IPAddress IPAddress
        {
            get { return ip; }
            set { ip = value; }
        }

        public IMachineCredentials TargetCredentials
        {
            set { wincred = (WindowsCredentials)value; }
        }

        public ScriptType SupportedScript => ScriptType.COMMAND;

        public PremiseType SupportedPremise => PremiseType.LOCAL;
        public bool IsSSL { set {} }
        public bool CheckCert { set {} }

        public string GetHostname()
        {
            throw new System.NotImplementedException();
        }

        public string GetOSVersion()
        {
            throw new System.NotImplementedException();
        }

        public bool IsConnectSuccessfull()
        {
            bool isrdp=false;
            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    tcpClient.Connect(ip, Convert.ToInt32(port));
                    isrdp = true;
                }
                catch (Exception)
                {
                    isrdp = false;
                }
            }
            return isrdp;
        }

        public string RunCommand(string command)
        {
            throw new System.NotImplementedException();
        }
    }
}