using System.Net;

namespace AssetDiscovery
{
    public interface ICommunication
    {
        string Name { get; }
        uint Port { get; set; }
        bool IsSSL { set; }
        bool CheckCert { set; }
        IPAddress IPAddress { get; set; }
        ScriptType SupportedScript { get; }
        PremiseType SupportedPremise { get; }
        IMachineCredentials TargetCredentials { set; }
        bool IsConnectSuccessfull();
        string GetHostname();
        string GetOSVersion();
        string RunCommand(string command);
    }
}