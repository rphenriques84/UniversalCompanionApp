namespace Companion.Backend.Api.Features.Games;

public sealed class GameDataOptions
{
    public const string SectionName = "GameData";

    /// <summary>Folder of game folders, relative to the app's own folder unless absolute.</summary>
    public string Directory { get; set; } = "data/games";
}
