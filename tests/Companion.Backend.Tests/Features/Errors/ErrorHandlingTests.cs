using System.Net;
using Companion.Backend.Api.Features.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;

namespace Companion.Backend.Tests.Features.Errors;

public sealed class ErrorHandlingTests
{
    private const string _secretDetail = "connection-string-secret";

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public async Task Request_UnhandledException_ReturnsProblemDetails(string environment)
    {
        await using var app = await StartAppAsync(environment);
        using var client = app.GetTestClient();

        using var response = await client.GetAsync(new Uri("/throw", UriKind.Relative));

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public async Task Request_UnhandledException_LeaksNoDetails(string environment)
    {
        await using var app = await StartAppAsync(environment);
        using var client = app.GetTestClient();

        using var response = await client.GetAsync(new Uri("/throw", UriKind.Relative));
        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain(_secretDetail, body, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(InvalidOperationException), body, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(ErrorHandlingTests), body, StringComparison.Ordinal);
    }

    private static async Task<WebApplication> StartAppAsync(string environment)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = environment });
        builder.WebHost.UseTestServer();
        builder.Services.AddErrorHandling();

        var app = builder.Build();
        app.UseErrorHandling();
        app.MapGet("/throw", ThrowingEndpoint);

        await app.StartAsync();
        return app;
    }

    private static IResult ThrowingEndpoint() => throw new InvalidOperationException(_secretDetail);
}
