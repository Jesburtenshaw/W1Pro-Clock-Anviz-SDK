using System.Threading.Channels;
using ClockTransactionsTransmiter.Devices.Connectivity;
using ClockTransactionsTransmiter.Devices.Events;
using Sense.Devices.Protocol.Cbor;
using Sense.Devices.Protocol.Common;
using Sense.Devices.Protocol.Envelopes;
using Sense.Devices.Protocol.Notifications;
using Sense.Devices.Protocol.Rpc;
using Sense.Devices.Protocol.Tiles;

namespace ClockTransactionsTransmiter.Devices;

public class SimulatedDevice : IAsyncDisposable
{
    public SimulatedDeviceSettings Settings { get; }

    private readonly DeviceToCloudConnection _cloudConnection;

    public ChannelReader<InternalDeviceEvent> Events => _eventsChannel.Reader;
    public ChannelReader<CloudRequest> Notifications => _notificationsChannel.Reader;

    public DeviceToCloudConnection Connection => _cloudConnection;

    private readonly Channel<InternalDeviceEvent> _eventsChannel;
    private readonly Channel<CloudRequest> _notificationsChannel;

    private readonly List<Tile> _tiles;

    public bool IsOn { get; private set; }
    public bool AirplaneMode => !_cloudConnection.IsConnected;

    public SimulatedDevice(SimulatedDeviceSettings settings, CborEncoder cborEncoder)
    {
        Settings = settings;

        _eventsChannel = Channel.CreateUnbounded<InternalDeviceEvent>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
            AllowSynchronousContinuations = false
        });

        _notificationsChannel = Channel.CreateUnbounded<CloudRequest>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
            AllowSynchronousContinuations = false
        });

        var connectionSettings = new DeviceToCloudConnectionSettings(
            deviceId: settings.DeviceId,
            ioTHubHostname: settings.IoTHubHostname,
            certificate: settings.Certificate);

        _cloudConnection = new DeviceToCloudConnection(connectionSettings, cborEncoder)
        {
            InboxHandler = OnMessageFromCloud,
            OutboxHandler = OnMessageFromDevice,
            DeviceTwinHandler = OnDeviceTwinUpdates,
            ErrorHandler = OnError
        };

        _tiles = new List<Tile>();
    }

    private Task OnDeviceTwinUpdates(SerializedDeviceTwinDesiredProperties desiredProperties)
    {
        return Task.CompletedTask;
    }

    public async Task Toggle(bool? desiredState, CancellationToken cancellationToken)
    {
        desiredState ??= !IsOn;

        if (desiredState.Value)
        {
            await TurnOn(cancellationToken);
        }
        else
        {
            await TurnOff(cancellationToken);
        }
    }

    public async Task TurnOn(CancellationToken cancellationToken)
    {
        if (IsOn)
        {
            return;
        }

        await _cloudConnection.Connect(cancellationToken);

        IsOn = true;

        await OnStateChanged();
    }

    public async Task TurnOff(CancellationToken cancellationToken)
    {
        if (!IsOn)
        {
            return;
        }

        await _cloudConnection.Disconnect(cancellationToken);

        IsOn = false;

        await OnStateChanged();
    }

    public async Task ToggleAirplaneMode(bool? desiredState, CancellationToken cancellationToken)
    {
        if (!IsOn)
        {
            throw new DeviceException("Device is turned off.");
        }

        desiredState ??= !_cloudConnection.IsConnected;

        if (desiredState.Value)
        {
            await _cloudConnection.Connect(cancellationToken);
        }
        else
        {
            await _cloudConnection.Disconnect(cancellationToken);
        }

        await OnStateChanged();
    }

    public Task<IReadOnlyList<Tile>> GetTiles(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Tile>>(_tiles);

    private async Task OnStateChanged() =>
        await _eventsChannel.Writer.WriteAsync(new DeviceStateChangedEvent());

    private async Task OnMessageFromDevice(byte[] payload, DeviceMessageEnvelope envelope) =>
        await _eventsChannel.Writer.WriteAsync(new DeviceSentMessageEvent(payload, envelope));

    private async Task OnMessageFromCloud(byte[] payload, CloudMessageEnvelope envelope)
    {
        await _eventsChannel.Writer.WriteAsync(new CloudSentMessageEvent(payload, envelope));

        if (envelope.Message.Body is CloudRequest { Body: PushNotification } request)
        {
            await _notificationsChannel.Writer.WriteAsync(request);
        }
    }

    private async Task OnError(DeviceException exception) =>
        await _eventsChannel.Writer.WriteAsync(new DeviceErrorEvent(exception));

    public async Task Send(DeviceMessageBody message, CancellationToken cancellationToken) =>
        await _cloudConnection.Send(message, cancellationToken);

    public async Task<CloudResponse> Request(DeviceRequest request, CancellationToken cancellationToken) =>
        await _cloudConnection.Request(request, cancellationToken);

    public async ValueTask DisposeAsync() => await TurnOff(CancellationToken.None);
}
