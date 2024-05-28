using System.Security.Cryptography.X509Certificates;

namespace ClockTransactionsTransmiter.Devices.Connectivity;

public sealed class DeviceToCloudConnectionSettings
{
    public string DeviceId { get; init; }
    public string IoTHubHostname { get; init; }
    public X509Certificate2 Certificate { get; init; }

    public DeviceToCloudConnectionSettings(string deviceId, string ioTHubHostname, X509Certificate2 certificate)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ArgumentException("No device ID specified.", nameof(deviceId));
        }

        if (string.IsNullOrWhiteSpace(ioTHubHostname))
        {
            throw new ArgumentException("No IoT hub hostname specified.", nameof(ioTHubHostname));
        }

        DeviceId = deviceId;
        IoTHubHostname = ioTHubHostname;
        Certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
    }
}
