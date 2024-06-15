using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ClockTransactionsTransmiter.Devices;

public static class DeviceCertificateLoader
{
    public static async Task<X509Certificate2> LoadDeviceCertificate(string certificate, string key)
    {
        using var publicKey = new X509Certificate2(Encoding.ASCII.GetBytes(certificate));

        var privateKeyBlocks = key.Split("-", StringSplitOptions.RemoveEmptyEntries);
        var privateKeyBytes = Convert.FromBase64String(privateKeyBlocks[1]);
        using var rsa = RSA.Create();

        switch (privateKeyBlocks[0])
        {
            case "BEGIN PRIVATE KEY":
                rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
                break;
            case "BEGIN RSA PRIVATE KEY":
                rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
                break;
        }

        var keyPair = publicKey.CopyWithPrivateKey(rsa);
        return new X509Certificate2(keyPair.Export(X509ContentType.Pfx));
    }
}
