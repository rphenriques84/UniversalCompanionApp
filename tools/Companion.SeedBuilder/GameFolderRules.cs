using System.Text.Json;

namespace Companion.SeedBuilder;

/// <summary>
/// Checks that each game folder has a game.json whose key matches the folder name.
/// Folder names are unique, so this also makes game keys unique.
/// </summary>
public static class GameFolderRules
{
    private const string _gameFile = "game.json";

    public static async Task<IReadOnlyList<SeedError>> ValidateAsync(string gamesDirectory, CancellationToken cancellationToken)
    {
        var errors = new List<SeedError>();

        foreach (var folder in Directory.EnumerateDirectories(gamesDirectory).Order())
        {
            var file = Path.Combine(folder, _gameFile);

            if (!File.Exists(file))
            {
                errors.Add(new SeedError(folder, $"Missing {_gameFile}."));
                continue;
            }

            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(file, cancellationToken));
            var key = document.RootElement.GetProperty("key").GetString();

            if (key != Path.GetFileName(folder))
            {
                errors.Add(new SeedError(file, $"Key '{key}' doesn't match its folder."));
            }
        }

        return errors;
    }
}
