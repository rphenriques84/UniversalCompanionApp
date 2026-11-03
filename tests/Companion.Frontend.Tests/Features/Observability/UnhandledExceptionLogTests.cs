using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Companion.Frontend.Tests.Features.Observability;

public sealed class UnhandledExceptionLogTests
{
    [Fact]
    public async Task Request_UnhandledException_LogsError()
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(host => host
            .UseEnvironment("Production")
            .UseSetting("Backend:BaseUrl", "http://backend:8080")
            .UseSetting("ServiceAuth:PrivateKey", Convert.ToBase64String(key.ExportPkcs8PrivateKey()))
            .ConfigureLogging(logging => logging.ClearProviders().AddFakeLogging())
            .ConfigureServices(services => services.AddTransient<IStartupFilter, ThrowAtEnd>()));
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/no-such-page", UriKind.Relative));

        var records = factory.Services.GetFakeLogCollector().GetSnapshot();
        Assert.Contains(records, record => record.Level == LogLevel.Error && record.Exception is InvalidOperationException);
    }

    /// <summary>Adds a failing step after the app's own pipeline, so the app's exception handler must catch it.</summary>
    private sealed class ThrowAtEnd : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            next(app);
            app.Run(_ => throw new InvalidOperationException("Test failure."));
        };
    }
}
