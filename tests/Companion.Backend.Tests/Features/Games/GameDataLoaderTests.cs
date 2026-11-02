using Companion.Backend.Data.Features.Games;
using Companion.Domain;

namespace Companion.Backend.Tests.Features.Games;

public sealed class GameDataLoaderTests : IDisposable
{
    private readonly string _gamesDirectory = Directory.CreateTempSubdirectory("game-data-tests").FullName;

    public void Dispose() => Directory.Delete(_gamesDirectory, recursive: true);

    [Fact]
    public async Task LoadAsync_ValidGames_ReturnsThemInFolderOrder()
    {
        AddGame("ffx", """{ "key": "ffx", "name": "Final Fantasy X" }""");
        AddGame("ff9", """{ "$schema": "x", "key": "ff9", "name": "Final Fantasy IX" }""");

        var games = await GameDataLoader.LoadAsync(_gamesDirectory, CancellationToken.None);

        Assert.Equal([new Game("ff9", "Final Fantasy IX"), new Game("ffx", "Final Fantasy X")], games);
    }

    [Theory]
    [InlineData("{ not json")]
    [InlineData("""{ "key": "ffx" }""")]
    [InlineData("""{ "key": "ffx", "name": null }""")]
    [InlineData("null")]
    public async Task LoadAsync_BadGameFile_Throws(string content)
    {
        AddGame("ffx", content);

        await Assert.ThrowsAsync<InvalidDataException>(() => GameDataLoader.LoadAsync(_gamesDirectory, CancellationToken.None));
    }

    [Fact]
    public async Task LoadAsync_FolderWithoutGameFile_Throws()
    {
        Directory.CreateDirectory(Path.Combine(_gamesDirectory, "ffx"));

        await Assert.ThrowsAsync<FileNotFoundException>(() => GameDataLoader.LoadAsync(_gamesDirectory, CancellationToken.None));
    }

    [Fact]
    public async Task LoadAsync_NoGames_Throws()
    {
        await Assert.ThrowsAsync<InvalidDataException>(() => GameDataLoader.LoadAsync(_gamesDirectory, CancellationToken.None));
    }

    [Fact]
    public async Task LoadAsync_FolderMissing_Throws()
    {
        var missing = Path.Combine(_gamesDirectory, "missing");

        await Assert.ThrowsAsync<DirectoryNotFoundException>(() => GameDataLoader.LoadAsync(missing, CancellationToken.None));
    }

    private void AddGame(string key, string content)
    {
        var folder = Directory.CreateDirectory(Path.Combine(_gamesDirectory, key));
        File.WriteAllText(Path.Combine(folder.FullName, "game.json"), content);
    }
}
