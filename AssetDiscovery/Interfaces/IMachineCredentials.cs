using System.Security;

namespace AssetDiscovery
{
    public interface IMachineCredentials
    {

    }

    public class WindowsCredentials : IMachineCredentials
    {
        public string Domain { get; set; }
        public string Username { get; set; }
        public SecureString Password { get; set; }
        public string CertificateThumbprint { get; set; }
    }

    public class UNIXCredentials : IMachineCredentials
    {
        public string Username { get; set; }
        public SecureString Password { get; set; }
        public string KeyFilePath { get; set; }
    }

    public class NetworkDeviceCredentials : IMachineCredentials
    {
        public string Username { get; set; }
        public SecureString Password { get; set; }
        public SecureString ENPassword { get; set; }
    }

    public class AWSCredentials : IMachineCredentials
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
    }
}
