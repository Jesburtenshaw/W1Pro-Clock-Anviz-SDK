using System.Security.Cryptography.X509Certificates;

namespace ClockTransactionsTransmiter.Devices;

public sealed class SimulatedDeviceSettings
{
    public string DeviceId { get; init; }
    public string IoTHubHostname { get; init; }
    public X509Certificate2 Certificate { get; init; }

    public SimulatedDeviceSettings(string deviceId, string ioTHubHostname, X509Certificate2 certificate)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(deviceId));
        }

        if (string.IsNullOrWhiteSpace(ioTHubHostname))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(ioTHubHostname));
        }

        DeviceId = deviceId;
        IoTHubHostname = ioTHubHostname;
        Certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
    }
}
