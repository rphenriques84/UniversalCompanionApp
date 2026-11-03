using System.Security.Cryptography;

namespace Companion.Frontend.Tests.Features.ServiceAuth;

/// <summary>Starts the real frontend process with a bad key. It must stop, and never print the key.</summary>
public sealed class StartupProcessTests
{
    // Valid Base64, so the value looks like a real key. It is not a P-256 private key.
    private static readonly string _badKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(138));

    [Fact]
    public async Task Start_BadPrivateKey_ExitsWithoutPrintingKey()
    {
        var run = await AppRun.StartAsync(new Dictionary<string, string>
        {
            ["ServiceAuth__PrivateKey"] = _badKey,
            ["Backend__BaseUrl"] = "http://127.0.0.1:1",
        });

        Assert.NotEqual(0, run.ExitCode);
        Assert.Contains("ServiceAuth:PrivateKey", run.Output, StringComparison.Ordinal);
        Assert.DoesNotContain(_badKey, run.Output, StringComparison.Ordinal);
    }
}
