using System.IO;

namespace ClockTransactionsTransmiter.Iothub;

public class BootargsParser
{
    private readonly string _content;
    
    private readonly Dictionary<string, string> _values;
    public string Filename { get; }

    public BootargsParser(string filename)
    {
        Filename  = filename;
        _content = File.ReadAllText(filename);
        _values = _content.Replace("bootargs>", "").Trim()
            .Split(",")
            .ToList()
            .ToDictionary(r => r.Substring(0, r.IndexOf("=")), r => r.Substring(r.IndexOf("=") + 1, r.Length - r.IndexOf("=") - 1));
    }

    public string DeviceName => _values["serial"];

    public string PublicKey => _values["crt"];
    public string PrivateKey => _values["key"];
    public string Hostname => _values["iothubname"];
}