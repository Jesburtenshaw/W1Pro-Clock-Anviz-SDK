using System.IO;
using System.Text;
using ClockTransactionsTransmiter.Devices.Connectivity;
using ClockTransactionsTransmiter.Iothub;
using Sense.Devices.Protocol.Cbor;
using Sense.Devices.Protocol.Events;
using Sense.Devices.Protocol.Events.ClockInOut;

namespace ClockTransactionsTransmiter.Devices;

public class IotHubPublisher
{
    private readonly BootargsParser bootargs;
    private DeviceToCloudConnection? _connection;

    public IotHubPublisher()
    {
        bootargs = new BootargsParser(Directory.GetFiles(Directory.GetCurrentDirectory(), "*.bootargs").Single());
    }

    private async Task CreateConnection()
    {
        var certificate = await DeviceCertificateLoader.LoadDeviceCertificate(bootargs.PublicKey, bootargs.PrivateKey);
        _connection =
            new DeviceToCloudConnection(
                new DeviceToCloudConnectionSettings(bootargs.DeviceName, bootargs.Hostname, certificate),
                new CborEncoder());

        await _connection.Connect(CancellationToken.None);
    }

    public async Task SendClockInOut(string employeeId, DateTimeOffset timestamp)
    {
        if (_connection == null || !_connection.IsConnected)
            await CreateConnection();

        await _connection.Send(new DeviceEvent(new ClockInReport(timestamp.Date, Encoding.UTF8.GetBytes(employeeId))),
            CancellationToken.None);
    }
}