using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Companion.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Companion.Backend.Tests.Features.ApiDocs;

public sealed class ApiDocsTests : IDisposable
{
    private static readonly Uri _documentUri = new("/openapi/v1.json", UriKind.Relative);
    private static readonly Uri _pageUri = new("/scalar", UriKind.Relative);
    private static readonly Uri _gamesUri = new("/games", UriKind.Relative);

    private readonly ECDsa _key = ECDsa.Create(ECCurve.NamedCurves.nistP256);

    public void Dispose() => _key.Dispose();

    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    public async Task GetDocs_OutsideDevelopmentWithoutToken_IsRejected(string environment)
    {
        var (document, _) = await GetAsync(environment, _documentUri);
        var (page, _) = await GetAsync(environment, _pageUri);

        Assert.Equal(HttpStatusCode.Unauthorized, document);
        Assert.Equal(HttpStatusCode.Unauthorized, page);
    }

    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    public async Task GetDocs_OutsideDevelopmentWithToken_ReturnsNotFound(string environment)
    {
        var (document, _) = await GetAsync(environment, _documentUri, CreateToken());
        var (page, _) = await GetAsync(environment, _pageUri, CreateToken());

        Assert.Equal(HttpStatusCode.NotFound, document);
        Assert.Equal(HttpStatusCode.NotFound, page);
    }

    [Fact]
    public async Task GetDocument_Development_ListsEndpointsAndBearerScheme()
    {
        var (status, content) = await GetAsync("Development", _documentUri);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("/games", content, StringComparison.Ordinal);
        Assert.Contains("bearer", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetPage_Development_ReturnsPage()
    {
        var (status, _) = await GetAsync("Development", _pageUri);

        Assert.Equal(HttpStatusCode.OK, status);
    }

    [Fact]
    public async Task GetGames_DevelopmentWithoutToken_IsRejected()
    {
        var (status, _) = await GetAsync("Development", _gamesUri);

        Assert.Equal(HttpStatusCode.Unauthorized, status);
    }

    [Fact]
    public async Task GetGames_DevelopmentWithToken_ReturnsOk()
    {
        var (status, _) = await GetAsync("Development", _gamesUri, CreateToken());

        Assert.Equal(HttpStatusCode.OK, status);
    }

    private async Task<(HttpStatusCode Status, string Content)> GetAsync(string environment, Uri uri, string? token = null)
    {
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(host => host
            .UseEnvironment(environment)
            .UseSetting("ServiceAuth:CurrentPublicKey", Convert.ToBase64String(_key.ExportSubjectPublicKeyInfo())));
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);

        if (token is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var response = await client.SendAsync(request);

        return (response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private string CreateToken() => new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
    {
        Issuer = ServiceTokens.Issuer,
        Audience = ServiceTokens.Audience,
        Expires = DateTime.UtcNow.AddMinutes(5),
        Claims = new Dictionary<string, object> { [ServiceTokens.ScopeClaim] = ServiceScopes.GameDataRead },
        SigningCredentials = new SigningCredentials(new ECDsaSecurityKey(_key), SecurityAlgorithms.EcdsaSha256),
    });
}
