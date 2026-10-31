using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Companion.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Companion.Backend.Tests.Features.Games;

public sealed class GamesEndpointsTests : IAsyncDisposable
{
    private static readonly Uri _gamesUri = new("/games", UriKind.Relative);

    private readonly ECDsa _key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    private readonly WebApplicationFactory<Program> _factory;

    public GamesEndpointsTests()
    {
        var publicKey = Convert.ToBase64String(_key.ExportSubjectPublicKeyInfo());

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(host => host.UseSetting("ServiceAuth:CurrentPublicKey", publicKey));
    }

    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();
        _key.Dispose();
    }

    [Fact]
    public async Task GetGames_ValidToken_ReturnsGames()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(ServiceScopes.GameDataRead));

        var games = await client.GetFromJsonAsync<GameDto[]>(_gamesUri);

        Assert.Contains(new GameDto("ffx", "Final Fantasy X HD Remaster"), games!);
    }

    [Fact]
    public async Task GetGames_MissingToken_IsRejected()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(_gamesUri);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetGames_WrongScope_IsForbidden()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken("other.scope"));

        using var response = await client.GetAsync(_gamesUri);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetGames_PipelineThrows_ReturnsProblemDetails()
    {
        // Authentication runs after the exception handler, so this proves Program.cs wires the handler first.
        // Production, because the Development error page also returns ProblemDetails.
        using var factory = _factory.WithWebHostBuilder(host => host.UseEnvironment("Production").ConfigureServices(services =>
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                options.Events = new JwtBearerEvents { OnMessageReceived = _ => throw new InvalidOperationException("pipeline failure") })));
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(_gamesUri);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    private string CreateToken(string scope) => new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
    {
        Issuer = ServiceTokens.Issuer,
        Audience = ServiceTokens.Audience,
        Expires = DateTime.UtcNow.AddMinutes(5),
        Claims = new Dictionary<string, object> { [ServiceTokens.ScopeClaim] = scope },
        SigningCredentials = new SigningCredentials(new ECDsaSecurityKey(_key), SecurityAlgorithms.EcdsaSha256),
    });
}
