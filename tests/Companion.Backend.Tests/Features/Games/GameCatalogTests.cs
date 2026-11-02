using System.Security.Cryptography;
using Companion.Backend.Api.Features.Games;
using Companion.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Companion.Backend.Tests.Features.Games;

public sealed class GameCatalogTests : IAsyncDisposable
{
    private readonly WebApplicationFactory<Program> _factory;

    public GameCatalogTests()
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var publicKey = Convert.ToBase64String(key.ExportSubjectPublicKeyInfo());

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(host => host.UseSetting("ServiceAuth:CurrentPublicKey", publicKey));
    }

    public async ValueTask DisposeAsync() => await _factory.DisposeAsync();

    [Fact]
    public void Startup_RealSeedData_LoadsGames()
    {
        using var client = _factory.CreateClient();

        var catalog = _factory.Services.GetRequiredService<GameCatalog>();

        Assert.Contains(new Game("ffx", "Final Fantasy X HD Remaster"), catalog.Games);
    }

    [Fact]
    public void Startup_BadGameData_FailsToStart()
    {
        using var factory = _factory.WithWebHostBuilder(host => host.UseSetting("GameData:Directory", "no-such-folder"));

        Assert.Throws<DirectoryNotFoundException>(() => factory.CreateClient());
    }
}
