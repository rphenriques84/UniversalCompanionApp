namespace Companion.SeedBuilder.Tests;

public sealed class SeedValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ValidGame_ReturnsNoErrors()
    {
        using var data = new SeedDataFolder();
        data.AddGameFile("game.json", """{ "key": "ffx", "name": "Final Fantasy X" }""");

        var errors = await SeedValidator.ValidateAsync(data.Path, CancellationToken.None);

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("""{ "name": "Final Fantasy X" }""")]
    [InlineData("""{ "key": "FFX", "name": "Final Fantasy X" }""")]
    [InlineData("""{ "key": "ffx", "name": "" }""")]
    [InlineData("""{ "key": "ffx", "name": "Final Fantasy X", "extra": 1 }""")]
    public async Task ValidateAsync_GameBreaksSchema_ReturnsError(string content)
    {
        using var data = new SeedDataFolder();
        data.AddGameFile("game.json", content);

        var errors = await SeedValidator.ValidateAsync(data.Path, CancellationToken.None);

        var error = Assert.Single(errors);
        Assert.EndsWith("game.json", error.File, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ValidateAsync_SeveralBreaks_ReturnsEveryError()
    {
        using var data = new SeedDataFolder();
        data.AddGameFile("game.json", """{ "key": "FFX", "name": "", "extra": 1 }""");

        var errors = await SeedValidator.ValidateAsync(data.Path, CancellationToken.None);

        Assert.Equal(3, errors.Count);
    }

    [Fact]
    public async Task ValidateAsync_KeyDiffersFromFolder_ReturnsError()
    {
        using var data = new SeedDataFolder();
        data.AddGameFile("game.json", """{ "key": "ffx-2", "name": "Final Fantasy X-2" }""");

        var errors = await SeedValidator.ValidateAsync(data.Path, CancellationToken.None);

        Assert.Equal("Key 'ffx-2' doesn't match its folder.", Assert.Single(errors).Message);
    }

    [Fact]
    public async Task ValidateAsync_FolderWithoutGameFile_ReturnsError()
    {
        using var data = new SeedDataFolder();
        data.AddEmptyGameFolder("ffx");

        var errors = await SeedValidator.ValidateAsync(data.Path, CancellationToken.None);

        Assert.Equal("Missing game.json.", Assert.Single(errors).Message);
    }

    [Fact]
    public async Task ValidateAsync_MalformedJson_ReturnsError()
    {
        using var data = new SeedDataFolder();
        data.AddGameFile("game.json", "{ not json");

        var errors = await SeedValidator.ValidateAsync(data.Path, CancellationToken.None);

        Assert.Equal("Not valid JSON.", Assert.Single(errors).Message);
    }

    [Fact]
    public async Task ValidateAsync_FileWithoutSchema_ReturnsError()
    {
        using var data = new SeedDataFolder();
        data.AddGameFile("unknown.json", "{}");

        var errors = await SeedValidator.ValidateAsync(data.Path, CancellationToken.None);

        Assert.StartsWith("No schema found", Assert.Single(errors).Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ValidateAsync_NoGamesFolder_ReturnsNoErrors()
    {
        using var data = new SeedDataFolder();

        var errors = await SeedValidator.ValidateAsync(data.Path, CancellationToken.None);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task ValidateAsync_DataFolderMissing_ReturnsError()
    {
        var errors = await SeedValidator.ValidateAsync("no-such-folder", CancellationToken.None);

        Assert.Equal("Folder not found.", Assert.Single(errors).Message);
    }
}
