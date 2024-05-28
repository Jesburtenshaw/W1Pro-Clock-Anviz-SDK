using System.Diagnostics;
using System.Security.Authentication;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Subscribing;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using Sense.Devices.Protocol;
using Sense.Devices.Protocol.Cbor;
using Sense.Devices.Protocol.Common;
using Sense.Devices.Protocol.Envelopes;
using Sense.Devices.Protocol.Rpc;

namespace ClockTransactionsTransmiter.Devices.Connectivity;

public sealed class DeviceToCloudConnection
{
    private readonly DeviceToCloudConnectionSettings _settings;
    private readonly CborEncoder _cborEncoder;
    private readonly IMqttClientOptions _mqttClientOptions;
    private readonly IMqttClient _mqttClient;
    private readonly RequestsManager _requests;

    public Func<byte[], DeviceMessageEnvelope, Task>? OutboxHandler { get; set; }
    public Func<byte[], CloudMessageEnvelope, Task>? InboxHandler { get; set; }
    public Func<SerializedDeviceTwinDesiredProperties, Task>? DeviceTwinHandler { get; set; }
    public Func<DeviceException, Task>? ErrorHandler { get; set; }

    public bool IsConnected => _mqttClient.IsConnected;

    public DeviceToCloudConnection(DeviceToCloudConnectionSettings settings, CborEncoder cborEncoder)
    {
        _settings = settings;
        _cborEncoder = cborEncoder;

        _mqttClientOptions = new MqttClientOptionsBuilder()
            .WithClientId(settings.DeviceId)
            .WithProtocolVersion(MqttProtocolVersion.V311)
            .WithTcpServer(settings.IoTHubHostname, 8883)
            .WithCredentials($"{settings.IoTHubHostname}/{settings.DeviceId}/?api-version=2018-06-30")
            .WithTls(options =>
            {
                options.UseTls = true;
                options.Certificates = new[] { settings.Certificate };
                // TODO: IDK if it matters
                options.SslProtocol = SslProtocols.Tls12;
            })
            .WithCleanSession(false)
            .Build();

        _mqttClient = new MqttFactory().CreateMqttClient();

        // TODO: Handlers for connection and disconnection events
        _mqttClient.UseApplicationMessageReceivedHandler(HandleMqttApplicationMessage);

        _requests = new RequestsManager(this);
    }

    public async Task Connect(CancellationToken cancellationToken)
    {
        if (_mqttClient.IsConnected)
        {
            return;
        }

        var result = await _mqttClient.ConnectAsync(_mqttClientOptions, cancellationToken);

        if (result.ResultCode != MqttClientConnectResultCode.Success)
        {
            throw new DeviceException($"Failed to connect: {result.ReasonString}");
        }

        await _mqttClient.SubscribeAsync(new MqttClientSubscribeOptions
        {
            TopicFilters = new List<MqttTopicFilter>
            {
                // Subscribe to cloud to device messages
                new()
                {
                    Topic = $"devices/{_settings.DeviceId}/messages/devicebound/#",
                    QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
                },
                // Subscribe to device twin responses
                new()
                {
                    Topic = "$iothub/twin/res/#",
                    QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
                },
                // Subscribe to device twin updates
                new()
                {
                    Topic = "$iothub/twin/PATCH/properties/desired/#",
                    QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
                }
            }
        }, CancellationToken.None);

        // Request the device twin
        await _mqttClient.PublishAsync($"$iothub/twin/GET/?$rid={Guid.NewGuid()}");
    }

    public Task<CloudResponse> Request(DeviceRequest request, CancellationToken cancellationToken) =>
        _requests.Request(request, cancellationToken);

    /// <summary>
    /// Simulates some data been sent by the device
    /// </summary>
    public Task<byte[]> Send(DeviceMessageBody body, CancellationToken cancellationToken)
    {
        var header = new MessageHeader(timestamp: DateTime.UtcNow);
        var envelope = new DeviceMessageEnvelope(
            messages: new DeviceMessage[]
            {
                // new DeviceMessage(new MessageHeader(DateTime.UtcNow),
                //     new DeviceEvent(new ImuEventReport(SensorCode.ACTIVITY_RECOGNITION))),
                new DeviceMessage(header, body)
            });
        return Send(envelope, cancellationToken);
    }

    public async Task<byte[]> Send(DeviceMessageEnvelope envelope, CancellationToken cancellationToken)
    {
        if (!IsConnected)
        {
            throw new DeviceException("Not connected.");
        }

        var watch = Stopwatch.StartNew();

        try
        {
            var data = _cborEncoder.Encode(envelope);

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic($"devices/{_settings.DeviceId}/messages/events/alerts")
                .WithPayload(data)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            watch.Start();
            await _mqttClient.PublishAsync(mqttMessage, cancellationToken);

            if (OutboxHandler != null)
            {
                await OutboxHandler(data, envelope);
            }

            return data;
        }
        catch (Exception exception)
            when (exception is DeviceContractViolationException or InvalidOperationException)
        {
            throw new DeviceException($"Failed to send envelope '{envelope.GetType().Name}'.", exception);
        }
        finally
        {
            watch.Stop();
            Console.WriteLine($"[DeviceToCloudConnection.Send] Elapsed: {watch.ElapsedMilliseconds}");
        }
    }

    public async Task HandleMqttApplicationMessage(MqttApplicationMessageReceivedEventArgs eventArgs)
    {
        // TODO: Better error handling here
        try
        {
            if (eventArgs.ApplicationMessage.Topic.StartsWith("$iothub/twin/res/"))
            {
                // This is a device twin response
                var deviceTwinJson = eventArgs.ApplicationMessage.ConvertPayloadToString();
                var deviceTwin = JsonConvert.DeserializeObject<SerializedDeviceTwin>(deviceTwinJson);

                if (deviceTwin == null || deviceTwin.Desired == null)
                {
                    return;
                }

                if (DeviceTwinHandler != null)
                {
                    await DeviceTwinHandler(deviceTwin!.Desired);
                }

                return;
            }

            if (eventArgs.ApplicationMessage.Topic.StartsWith("$iothub/twin/PATCH/properties/desired/"))
            {
                // This is a device twin update
                var deviceTwinUpdateJson = eventArgs.ApplicationMessage.ConvertPayloadToString();

                var deviceTwinUpdate =
                    JsonConvert.DeserializeObject<SerializedDeviceTwinDesiredProperties>(deviceTwinUpdateJson);

                if (deviceTwinUpdate == null)
                {
                    return;
                }

                if (DeviceTwinHandler != null)
                {
                    await DeviceTwinHandler(deviceTwinUpdate!);
                }

                return;
            }
        }
        catch (Exception ex)
        {
            if (ErrorHandler != null)
            {
                await ErrorHandler(new DeviceException("Failed to read the device twin", ex));
            }
        }

        CloudMessageEnvelope? message = await Decode(eventArgs.ApplicationMessage.Payload);

        if (message != null)
        {
            await _requests.Handle(message, CancellationToken.None);

            if (InboxHandler != null)
            {
                await InboxHandler(eventArgs.ApplicationMessage.Payload, message);
            }
        }
    }

    private async Task<CloudMessageEnvelope?> Decode(byte[] payload)
    {
        try
        {
            return _cborEncoder.Decode<CloudMessageEnvelope>(payload);
        }
        catch (Exception exception)
            when (exception is DeviceContractViolationException or InvalidOperationException)
        {
            if (ErrorHandler != null)
            {
                await ErrorHandler(new DeviceException("Failed to receive message.", exception)
                {
                    ReceivedDataAsHex = BitConverter.ToString(payload).Replace("-", string.Empty)
                });
            }

            return null;
        }
    }

    public async Task Disconnect(CancellationToken cancellationToken)
    {
        if (!_mqttClient.IsConnected)
        {
            return;
        }

        await _mqttClient.DisconnectAsync(cancellationToken);
    }
}
