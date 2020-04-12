using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Net.Sockets;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;

namespace AssetDiscovery.Communicators
{
    public class WinRMCommunication : ICommunication
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private uint port = 5985;
        private IPAddress ip;
        private WindowsCredentials wincred;
        private CimSession Session = null;
        private bool ssl = false;
        private bool checkcert = true;
        public string Name => "WINRM";

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

        public bool IsSSL { set { ssl = value; } }
        public bool CheckCert { set { checkcert = value; } }

        public string GetHostname()
        {
            string hostname = string.Empty;
            if (string.IsNullOrEmpty(wincred.Domain))
            {
                wincred.Domain = ip.ToString();
            }
            CimCredential Credentials = new CimCredential(PasswordAuthenticationMechanism.Default, wincred.Domain, wincred.Username, wincred.Password);
            WSManSessionOptions SessionOptions = new WSManSessionOptions();
            SessionOptions.AddDestinationCredentials(Credentials);
            if (ssl)
            {
                SessionOptions.UseSsl = true;
            }
            else
            {
                SessionOptions.UseSsl = false;
            }
            SessionOptions.DestinationPort = port;
            if (checkcert)
            {
                SessionOptions.CertCACheck = true;
                SessionOptions.CertCNCheck = true;
                SessionOptions.CertRevocationCheck = true;
            }
            else
            {
                SessionOptions.CertCACheck = false;
                SessionOptions.CertCNCheck = false;
                SessionOptions.CertRevocationCheck = false;
            }

            try
            {
                Session = CimSession.Create(ip.ToString(), SessionOptions);
                CimInstance searchInstance = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT CSNAME from win32_operatingsystem").FirstOrDefault();
                if (searchInstance != null)
                {
                    hostname = searchInstance.CimInstanceProperties["CSName"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                log.Error("IPADDRESS: " + ip + " ;MESSAGE: " + ex.Message);
            }
            return hostname;
        }

        public string GetOSVersion()
        {
            string osversion = string.Empty;
            try
            {
                CimInstance searchInstance = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * from win32_operatingsystem").FirstOrDefault();
                if (searchInstance != null)
                {
                    osversion = searchInstance.CimInstanceProperties["Caption"].Value.ToString() + " : " + searchInstance.CimInstanceProperties["Version"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                log.Error("IPADDRESS: " + ip + " ;MESSAGE: " + ex.Message);
            }
            return osversion;
        }

        public bool IsConnectSuccessfull()
        {
            bool iswinrm = false;
            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    tcpClient.Connect(ip, Convert.ToInt32(port));
                    iswinrm = true;
                }
                catch (Exception)
                {
                    iswinrm = false;
                }
            }
            return iswinrm;
        }

        public string RunCommand(string command)
        {
            string result = string.Empty;
            string username = wincred.Username;
            string password = new System.Net.NetworkCredential(string.Empty, wincred.Password).Password;
            IEnumerable<PSObject> results = null;

            using (var ps = PowerShell.Create())
            {
                ps.AddScript("$pass = ConvertTo-SecureString -String \"" + password + "\" -AsPlainText -Force");
                ps.AddScript("$user = \"" + username + "\"");
                ps.AddScript("$cred = New-Object System.Management.Automation.PSCredential -ArgumentList $user,$pass");
                string postscript = string.Empty;
                if (!checkcert)
                {
                    postscript = "-SessionOption (New-PSSessionOption -SkipCACheck -SkipCNCheck -SkipRevocationCheck)";
                }
                if (ssl)
                {
                    ps.AddScript("Invoke-Command -ComputerName " + ip.ToString() + " -Credential $cred -Port " + port + " -UseSSL -ScriptBlock {" + command + "} " + postscript);
                }
                else
                {
                    ps.AddScript("Invoke-Command -ComputerName " + ip.ToString() + " -Credential $cred -Port " + port + " -ScriptBlock {" + command + "}");
                }
                results = ps.Invoke();
                if (ps.HadErrors)
                {
                    IEnumerable<ErrorRecord> err = ps.Streams.Error.ReadAll();
                    foreach (ErrorRecord er in err)
                    {
                        if (er.Exception.Message.Contains("Verify that the specified computer name is valid"))
                        {
                            log.Error("IPADDRESS: " + ip + "; MESSAGE: RPC Server Unavailable");
                        }
                        else if (er.Exception.Message.Contains("Access is denied"))
                        {
                            log.Error("IPADDRESS: " + ip + "; MESSAGE: ACCESS DENIED");
                        }
                        else if (er.Exception.Message == "Cannot convert value \"Ssl3,Tls,Tls11,Tls12\" to type \"System.Net.SecurityProtocolType\" due to invalid enumeration values. Specify one of the following enumeration values and try again. The possible enumeration values are \"Ssl3, Tls\".")
                        {
                            log.Error("IPADDRESS: " + ip + "; MESSAGE: Crypto Algorithm error");
                        }
                        else
                        {
                            log.Error("IPADDRESS: " + ip + "; MESSAGE: " + er.Exception.Message);
                        }
                    }
                }
            }
            foreach (PSObject r in results)
            {
                result += r.ToString();
            }
            return result;
        }
    }
}