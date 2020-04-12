using System;
using System.Net;
using System.Net.Sockets;

namespace AssetDiscovery.Communicators
{
    public class TelnetCommunication : ICommunication
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private uint port = 22;
        private IPAddress ip;
        private NetworkDeviceCredentials nwcred;
        public string Name => "TELNET";

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
            set { nwcred = (NetworkDeviceCredentials)value; }
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
            bool istelnet=false;
            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    tcpClient.Connect(ip, Convert.ToInt32(port));
                    istelnet = true;
                }
                catch (Exception)
                {
                    istelnet = false;
                }
            }
            return istelnet;
        }

        public string RunCommand(string command)
        {
            throw new System.NotImplementedException();
        }
    }
}