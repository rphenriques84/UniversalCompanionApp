using Companion.Backend.Data.Features.Games;
using Companion.Domain;
using Microsoft.Extensions.Options;

namespace Companion.Backend.Api.Features.Games;

/// <summary>Holds the game data in memory. Loads it at startup, before the server takes requests.</summary>
public sealed partial class GameCatalog(IOptions<GameDataOptions> options, ILogger<GameCatalog> logger) : IHostedService
{
    public IReadOnlyList<Game> Games { get; private set; } = [];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var directory = Path.Combine(AppContext.BaseDirectory, options.Value.Directory);

        Games = await GameDataLoader.LoadAsync(directory, cancellationToken);

        LogGamesLoaded(Games.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(Level = LogLevel.Information, Message = "Loaded {Count} games")]
    private partial void LogGamesLoaded(int count);
}
