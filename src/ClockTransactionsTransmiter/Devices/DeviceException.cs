namespace ClockTransactionsTransmiter.Devices;

public class DeviceException : Exception
{
    public string? ReceivedDataAsHex { get; set; }

    public DeviceException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}
