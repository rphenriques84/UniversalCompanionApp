namespace Companion.SeedBuilder.Tests;

public sealed class SeedCommandTests
{
    [Theory]
    [InlineData]
    [InlineData("unknown")]
    [InlineData("validate")]
    public async Task RunAsync_BadArguments_ReturnsInvalidUsage(params string[] args)
    {
        using var error = new StringWriter();

        var exitCode = await SeedCommand.RunAsync(args, TextWriter.Null, error, CancellationToken.None);

        Assert.Equal(SeedCommand.InvalidUsage, exitCode);
        Assert.Contains("Usage", error.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_ValidData_ReturnsSuccess()
    {
        using var data = new SeedDataFolder();
        data.AddGameFile("game.json", """{ "key": "ffx", "name": "Final Fantasy X" }""");

        var exitCode = await SeedCommand.RunAsync(["validate", data.Path], TextWriter.Null, TextWriter.Null, CancellationToken.None);

        Assert.Equal(SeedCommand.Success, exitCode);
    }

    [Fact]
    public async Task RunAsync_InvalidData_ReturnsInvalidDataAndPrintsError()
    {
        using var data = new SeedDataFolder();
        data.AddGameFile("game.json", "{}");
        using var error = new StringWriter();

        var exitCode = await SeedCommand.RunAsync(["validate", data.Path], TextWriter.Null, error, CancellationToken.None);

        Assert.Equal(SeedCommand.InvalidData, exitCode);
        Assert.Contains("game.json", error.ToString(), StringComparison.Ordinal);
    }
}
