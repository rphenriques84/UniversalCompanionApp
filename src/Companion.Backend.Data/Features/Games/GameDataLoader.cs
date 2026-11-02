using System.Text.Json;
using Companion.Domain;

namespace Companion.Backend.Data.Features.Games;

/// <summary>Reads the game.json of each game folder. Bad data throws, so the backend doesn't start with it.</summary>
public static class GameDataLoader
{
    private const string _gameFile = "game.json";

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
    };

    public static async Task<IReadOnlyList<Game>> LoadAsync(string gamesDirectory, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(gamesDirectory))
        {
            throw new DirectoryNotFoundException($"Game data folder not found: {gamesDirectory}");
        }

        var games = new List<Game>();

        foreach (var folder in Directory.EnumerateDirectories(gamesDirectory).Order())
        {
            games.Add(await LoadGameAsync(Path.Combine(folder, _gameFile), cancellationToken));
        }

        return games.Count > 0 ? games : throw new InvalidDataException($"No games found in {gamesDirectory}");
    }

    private static async Task<Game> LoadGameAsync(string file, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(file);

        try
        {
            return await JsonSerializer.DeserializeAsync<Game>(stream, _jsonOptions, cancellationToken)
                ?? throw new InvalidDataException($"Game file holds no game: {file}");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"Game file is not valid: {file}", exception);
        }
    }
}
