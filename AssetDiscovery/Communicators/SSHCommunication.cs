using System;
using System.Net;
using System.Net.Sockets;
using Renci.SshNet;

namespace AssetDiscovery.Communicators
{
    public class SSHCommunication : ICommunication
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private uint port = 22;
        private IPAddress ip;
        private UNIXCredentials unixcred;
        private SshClient sshclient;
        public string Name => "SSH";
        ConnectionInfo conn = null;

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
            set { unixcred = (UNIXCredentials)value; }
        }

        public ScriptType SupportedScript => ScriptType.BASH;

        public PremiseType SupportedPremise => PremiseType.LOCAL;

        public bool IsSSL { set { } }
        public bool CheckCert { set { } }

        public string GetHostname()
        {
            string ret = string.Empty;
            string password = new NetworkCredential(string.Empty, unixcred.Password).Password;
            conn = new ConnectionInfo(ip.ToString(), unixcred.Username,
                new AuthenticationMethod[]{
                        new PasswordAuthenticationMethod(unixcred.Username,password)
                }
            );

            try
            {
                sshclient = new SshClient(conn);
                sshclient.Connect();
                using (var cmd = sshclient.CreateCommand("hostname"))
                {
                    cmd.Execute();
                    string result = cmd.Result;
                    if (!string.IsNullOrEmpty(result))
                    {
                        ret = result;
                    }
                }
                // sshclient.Disconnect();

            }
            catch (Exception ex)
            {
                log.Error("IPADDRESS: " + ip + ";MESSAGE: " + ex.Message);
            }

            return ret;
        }

        public string GetOSVersion()
        {
            string ret = string.Empty;
            try
            {
                using (var cmd = sshclient.CreateCommand("uname -a"))
                {
                    cmd.Execute();
                    string result = cmd.Result;
                    if (!string.IsNullOrEmpty(result))
                    {
                        if (!result.ToLower().Contains("invalid"))
                        {
                            ret = result;
                        }
                    }
                }
                sshclient.Disconnect();
            }
            catch (Exception ex)
            {
                log.Error("IPADDRESS: " + ip + ";MESSAGE: " + ex.Message);
            }
            return ret;
        }

        public bool IsConnectSuccessfull()
        {
            bool isssh = false;
            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    tcpClient.Connect(ip, Convert.ToInt32(port));
                    isssh = true;
                }
                catch (Exception)
                {
                    isssh = false;
                }
            }
            return isssh;
        }

        public string RunCommand(string command)
        {
            string ret = string.Empty;
            try
            {
                if (!sshclient.IsConnected)
                {
                    sshclient = new SshClient(conn);
                    sshclient.Connect();
                }
                using (var cmd = sshclient.CreateCommand(command))
                {
                    cmd.Execute();
                    string result = cmd.Result;
                    if (!string.IsNullOrEmpty(result))
                    {
                        if (!result.ToLower().Contains("invalid"))
                        {
                            ret = result;
                        }
                    }
                }
                // sshclient.Disconnect();
            }
            catch (Exception ex)
            {
                log.Error("IPADDRESS: " + ip + ";MESSAGE: " + ex.Message);
            }
            return ret;
        }
    }
}