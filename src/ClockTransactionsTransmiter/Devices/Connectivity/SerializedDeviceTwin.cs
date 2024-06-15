using Newtonsoft.Json;

namespace ClockTransactionsTransmiter.Devices.Connectivity;

public sealed class SerializedDeviceTwin

{
    [JsonProperty("desired")]
    public SerializedDeviceTwinDesiredProperties Desired { get; set; } = null!;
}
