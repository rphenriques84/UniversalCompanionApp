using System.Security.Cryptography;
using Companion.Backend.Api.Features.Games;
using Companion.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

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
    public async Task StartAsync_BadGameData_Throws()
    {
        // Calls the catalog directly. Through WebApplicationFactory, a failed start can race with host disposal.
        var options = Options.Create(new GameDataOptions { Directory = "no-such-folder" });
        var catalog = new GameCatalog(options, NullLogger<GameCatalog>.Instance);

        await Assert.ThrowsAsync<DirectoryNotFoundException>(() => catalog.StartAsync(CancellationToken.None));
    }
}
