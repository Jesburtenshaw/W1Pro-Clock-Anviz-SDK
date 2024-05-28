using Sense.Devices.Protocol.Envelopes;

namespace ClockTransactionsTransmiter.Devices.Events;

public sealed record CloudSentMessageEvent(byte[] Payload, CloudMessageEnvelope Envelope) : InternalDeviceEvent;
