using System;
using System.Net;
using System.Net.Sockets;

namespace AssetDiscovery.Communicators
{
    public class WMICommunication : ICommunication
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private uint port = 0;
        private IPAddress ip;
        private WindowsCredentials wincred;
        public string Name => "WMI";

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

        public ScriptType SupportedScript => ScriptType.POWERSHELL;

        public PremiseType SupportedPremise => PremiseType.LOCAL;
        public bool IsSSL { set { } }
        public bool CheckCert { set { } }

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
            bool iswmi = false;
            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    tcpClient.Connect(ip, Convert.ToInt32(port));
                    iswmi = true;
                }
                catch (Exception)
                {
                    iswmi = false;
                }
            }
            return iswmi;
        }

        public string RunCommand(string command)
        {
            throw new System.NotImplementedException();
        }
    }
}