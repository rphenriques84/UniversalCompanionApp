using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Companion.Architecture.Tests;

public sealed class GenerateServiceKeysTests : IDisposable
{
    private const string _settings = "ASPNETCORE_ENVIRONMENT=Development\n";

    private readonly string _envFile = Path.GetTempFileName();

    public void Dispose() => File.Delete(_envFile);

    [Fact]
    public async Task Run_NewFile_WritesUtf8WithoutByteOrderMark()
    {
        File.WriteAllText(_envFile, _settings);

        await RunAsync();

        var bytes = File.ReadAllBytes(_envFile);
        Assert.Equal((byte)'A', bytes[0]);
        Assert.DoesNotContain((byte)0, bytes);
    }

    [Fact]
    public async Task Run_NewFile_KeepsSettingsAndAddsMatchingKeys()
    {
        File.WriteAllText(_envFile, _settings);

        await RunAsync();

        var lines = File.ReadAllLines(_envFile);
        Assert.Equal(_settings.TrimEnd(), lines[0]);

        using var privateKey = ECDsa.Create();
        privateKey.ImportPkcs8PrivateKey(Convert.FromBase64String(ValueOf(lines, "SERVICE_TOKEN_PRIVATE_KEY")), out _);
        var expectedPublicKey = Convert.ToBase64String(privateKey.ExportSubjectPublicKeyInfo());

        Assert.Equal(256, privateKey.KeySize);
        Assert.Equal(expectedPublicKey, ValueOf(lines, "SERVICE_TOKEN_PUBLIC_KEY"));
    }

    [Fact]
    public async Task Run_NewFile_DoesNotPrintKeys()
    {
        File.WriteAllText(_envFile, _settings);

        var result = await RunAsync();

        var privateKey = ValueOf(File.ReadAllLines(_envFile), "SERVICE_TOKEN_PRIVATE_KEY");
        Assert.DoesNotContain(privateKey, result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Run_FileWithoutFinalNewline_StartsKeysOnNewLine()
    {
        File.WriteAllText(_envFile, _settings.TrimEnd());

        await RunAsync();

        Assert.Equal(3, File.ReadAllLines(_envFile).Length);
    }

    [Fact]
    public async Task Run_Utf16File_RewritesAsUtf8()
    {
        File.WriteAllText(_envFile, _settings, Encoding.Unicode);

        await RunAsync();

        Assert.DoesNotContain((byte)0, File.ReadAllBytes(_envFile));
    }

    [Fact]
    public async Task Run_KeysAlreadyPresent_FailsAndChangesNothing()
    {
        File.WriteAllText(_envFile, _settings);
        await RunAsync();
        var original = File.ReadAllText(_envFile);

        var result = await RunAsync();

        Assert.Equal(1, result.ExitCode);
        Assert.Equal(original, File.ReadAllText(_envFile));
    }

    [Fact]
    public async Task Run_MissingFile_Fails()
    {
        File.Delete(_envFile);

        var result = await RunAsync();

        Assert.Equal(1, result.ExitCode);
        Assert.False(File.Exists(_envFile));
    }

    private static string ValueOf(string[] lines, string name) =>
        lines.Single(line => line.StartsWith($"{name}=", StringComparison.Ordinal))[(name.Length + 1)..];

    private async Task<ToolResult> RunAsync()
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            ArgumentList = { "tools/generate-service-keys.cs", _envFile },
            WorkingDirectory = ProjectFile.FindRepositoryRoot(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        using var process = Process.Start(startInfo)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return new ToolResult(process.ExitCode, await output + await error);
    }

    private sealed record ToolResult(int ExitCode, string Output);
}
