using Sense.Devices.Protocol.Envelopes;

namespace ClockTransactionsTransmiter.Devices.Events;

public sealed record DeviceSentMessageEvent(byte[] Payload, DeviceMessageEnvelope Envelope) : InternalDeviceEvent;
