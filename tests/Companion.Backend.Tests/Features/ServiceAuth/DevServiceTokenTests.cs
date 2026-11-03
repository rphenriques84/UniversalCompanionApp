using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Companion.Backend.Tests.Features.ServiceAuth;

/// <summary>Runs tools/dev-service-token.cs as a real process, against a test .env in a temp folder.</summary>
public sealed class DevServiceTokenTests : IDisposable
{
    private static readonly Uri _gamesUri = new("/games", UriKind.Relative);

    private readonly ECDsa _key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    private readonly DirectoryInfo _workingDirectory = Directory.CreateTempSubdirectory("companion-token-");
    private readonly string _repositoryRoot = FindRepositoryRoot();

    public DevServiceTokenTests() =>
        // Same SDK as the repo, because the tool runs outside it.
        File.Copy(Path.Combine(_repositoryRoot, "global.json"), Path.Combine(_workingDirectory.FullName, "global.json"));

    public void Dispose()
    {
        _key.Dispose();
        _workingDirectory.Delete(recursive: true);
    }

    [Fact]
    public async Task Run_ValidKey_TokenAcceptedByBackend()
    {
        WriteSecrets($"SERVICE_TOKEN_PRIVATE_KEY={PrivateKey}\n");

        var result = await RunAsync();

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(HttpStatusCode.OK, await GetGamesAsync(result.Output.Trim()));
    }

    [Fact]
    public async Task Run_OtherScope_TokenRejectedByGamesEndpoint()
    {
        WriteSecrets($"SERVICE_TOKEN_PRIVATE_KEY={PrivateKey}\n");

        var result = await RunAsync("other.scope");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(HttpStatusCode.Forbidden, await GetGamesAsync(result.Output.Trim()));
    }

    [Fact]
    public async Task Run_ValidKey_TokenExpiresInFiveMinutes()
    {
        WriteSecrets($"SERVICE_TOKEN_PRIVATE_KEY={PrivateKey}\n");

        var result = await RunAsync();

        var token = new JsonWebToken(result.Output.Trim());
        Assert.Equal(TimeSpan.FromMinutes(5), token.ValidTo - token.IssuedAt);
    }

    [Fact]
    public async Task Run_ValidKey_DoesNotPrintKey()
    {
        WriteSecrets($"SERVICE_TOKEN_PRIVATE_KEY={PrivateKey}\n");

        var result = await RunAsync();

        Assert.DoesNotContain(PrivateKey, result.Output + result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Run_NoKeyInFile_FailsWithoutToken()
    {
        WriteSecrets("SERVICE_TOKEN_PUBLIC_KEY=public\n");

        var result = await RunAsync();

        Assert.Equal(1, result.ExitCode);
        Assert.Empty(result.Output);
    }

    [Fact]
    public async Task Run_NoFile_FailsWithoutToken()
    {
        var result = await RunAsync();

        Assert.Equal(1, result.ExitCode);
        Assert.Empty(result.Output);
    }

    private string PrivateKey => Convert.ToBase64String(_key.ExportPkcs8PrivateKey());

    private void WriteSecrets(string content) => File.WriteAllText(Path.Combine(_workingDirectory.FullName, ".env"), content);

    private async Task<ToolResult> RunAsync(params string[] arguments)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = _workingDirectory.FullName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        startInfo.ArgumentList.Add(Path.Combine(_repositoryRoot, "tools", "dev-service-token.cs"));
        arguments.ToList().ForEach(startInfo.ArgumentList.Add);

        using var process = Process.Start(startInfo)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return new ToolResult(process.ExitCode, await output, await error);
    }

    private async Task<HttpStatusCode> GetGamesAsync(string token)
    {
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(host => host
            .UseSetting("ServiceAuth:CurrentPublicKey", Convert.ToBase64String(_key.ExportSubjectPublicKeyInfo())));
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, _gamesUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request);

        return response.StatusCode;
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Companion.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Repository root not found.");
    }

    private sealed record ToolResult(int ExitCode, string Output, string Error);
}
