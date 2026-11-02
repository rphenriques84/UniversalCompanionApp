using System.Net;
using System.Net.Http.Json;
using Companion.Contracts;

namespace Companion.E2E;

public sealed class BffGamesTests
{
    private static readonly Uri _gamesUri = new("http://127.0.0.1:8080/bff/games");
    private static readonly Uri _stylesheetUri = new("http://127.0.0.1:8080/css/app.out.css");
    private static readonly Uri _documentUri = new("http://127.0.0.1:8080/openapi/v1.json");
    private static readonly Uri _pageUri = new("http://127.0.0.1:8080/scalar");

    [E2EFact]
    public async Task GetGames_RealStack_ReturnsGamesFromBackend()
    {
        await using var stack = await ComposeStack.StartAsync();
        using var client = new HttpClient();

        var games = await client.GetFromJsonAsync<GameDto[]>(_gamesUri);

        Assert.Contains(new GameDto("ffx", "Final Fantasy X HD Remaster"), games!);
    }

    [E2EFact]
    public async Task GetStylesheet_RealStack_ReturnsTailwindOutput()
    {
        await using var stack = await ComposeStack.StartAsync();
        using var client = new HttpClient();

        var stylesheet = await client.GetStringAsync(_stylesheetUri);

        Assert.Contains("tailwindcss", stylesheet, StringComparison.Ordinal);
        Assert.Contains("--theme-background", stylesheet, StringComparison.Ordinal);
    }

    [E2EFact]
    public async Task GetDocs_RealStack_ReturnsNotFound()
    {
        await using var stack = await ComposeStack.StartAsync();
        using var client = new HttpClient();

        using var document = await client.GetAsync(_documentUri);
        using var page = await client.GetAsync(_pageUri);

        Assert.Equal(HttpStatusCode.NotFound, document.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, page.StatusCode);
    }
}
