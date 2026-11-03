using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Companion.Backend.Tests.Features.Observability;

/// <summary>Checks the shipped production log levels, which keep telemetry within free-tier limits.</summary>
public sealed class LogLevelTests
{
    [Theory]
    [InlineData("Companion.Backend.Api.Features.Games.GameCatalog", LogLevel.Information)]
    [InlineData("Microsoft.Hosting.Lifetime", LogLevel.Information)]
    [InlineData("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning)]
    [InlineData("System.Net.Http.HttpClient", LogLevel.Warning)]
    public async Task Production_Category_HasExpectedMinimumLevel(string category, LogLevel minimum)
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(host => host
            .UseEnvironment("Production")
            .UseSetting("ServiceAuth:CurrentPublicKey", Convert.ToBase64String(key.ExportSubjectPublicKeyInfo())));

        var logger = factory.Services.GetRequiredService<ILoggerFactory>().CreateLogger(category);

        Assert.True(logger.IsEnabled(minimum));
        Assert.False(logger.IsEnabled(minimum - 1));
    }
}
