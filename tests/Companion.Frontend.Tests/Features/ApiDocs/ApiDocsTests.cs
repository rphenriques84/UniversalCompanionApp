using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Companion.Frontend.Tests.Features.ApiDocs;

public sealed class ApiDocsTests
{
    private static readonly Uri _documentUri = new("/openapi/v1.json", UriKind.Relative);
    private static readonly Uri _pageUri = new("/scalar", UriKind.Relative);

    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    public async Task GetDocs_OutsideDevelopment_ReturnsNotFound(string environment)
    {
        var document = await GetAsync(environment, _documentUri);
        var page = await GetAsync(environment, _pageUri);

        Assert.Equal(HttpStatusCode.NotFound, document.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, page.StatusCode);
    }

    [Fact]
    public async Task GetDocument_Development_ListsBffEndpoints()
    {
        var response = await GetAsync("Development", _documentUri);

        Assert.Contains("/bff/games", response.Content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetPage_Development_ReturnsPage()
    {
        var response = await GetAsync("Development", _pageUri);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<(HttpStatusCode StatusCode, string Content)> GetAsync(string environment, Uri uri)
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(host => host
            .UseEnvironment(environment)
            .UseSetting("Backend:BaseUrl", "http://backend:8080")
            .UseSetting("ServiceAuth:PrivateKey", Convert.ToBase64String(key.ExportPkcs8PrivateKey())));
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(uri);

        return (response.StatusCode, await response.Content.ReadAsStringAsync());
    }
}
