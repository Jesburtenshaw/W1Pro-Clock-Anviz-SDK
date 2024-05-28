namespace ClockTransactionsTransmiter.Devices.Events;

public sealed record DeviceErrorEvent(DeviceException Exception) : InternalDeviceEvent;
