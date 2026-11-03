using System.Security.Cryptography;

namespace Companion.Backend.Tests.Features.ServiceAuth;

/// <summary>Starts the real backend process with bad settings. It must stop, and never print a key.</summary>
public sealed class StartupProcessTests
{
    // Valid Base64, so the value looks like a real key. It is not a P-256 public key.
    private static readonly string _badKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(91));

    [Fact]
    public async Task Start_BadCurrentKey_ExitsWithoutPrintingKey()
    {
        var run = await AppRun.StartAsync(new Dictionary<string, string> { ["ServiceAuth__CurrentPublicKey"] = _badKey });

        Assert.NotEqual(0, run.ExitCode);
        Assert.Contains("ServiceAuth:CurrentPublicKey", run.Output, StringComparison.Ordinal);
        Assert.DoesNotContain(_badKey, run.Output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Start_BadPreviousKey_ExitsWithoutPrintingKey()
    {
        var run = await AppRun.StartAsync(new Dictionary<string, string>
        {
            ["ServiceAuth__CurrentPublicKey"] = CreatePublicKey(),
            ["ServiceAuth__PreviousPublicKey"] = _badKey,
        });

        Assert.NotEqual(0, run.ExitCode);
        Assert.Contains("ServiceAuth:PreviousPublicKey", run.Output, StringComparison.Ordinal);
        Assert.DoesNotContain(_badKey, run.Output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Start_BadGameDataDirectory_Exits()
    {
        var run = await AppRun.StartAsync(new Dictionary<string, string>
        {
            ["ServiceAuth__CurrentPublicKey"] = CreatePublicKey(),
            ["GameData__Directory"] = "no-such-folder",
        });

        Assert.NotEqual(0, run.ExitCode);
        Assert.Contains("no-such-folder", run.Output, StringComparison.Ordinal);
    }

    private static string CreatePublicKey()
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        return Convert.ToBase64String(key.ExportSubjectPublicKeyInfo());
    }
}
