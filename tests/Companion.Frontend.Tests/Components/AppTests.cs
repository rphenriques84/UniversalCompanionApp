using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Companion.Frontend.Tests.Components;

public sealed class AppTests
{
    private static readonly Uri _homeUri = new("/", UriKind.Relative);
    private static readonly Uri _errorUri = new("/Error", UriKind.Relative);

    [Fact]
    public async Task GetHome_Anonymous_UsesNeutralTheme()
    {
        var page = await GetPageAsync(_homeUri);

        Assert.Contains("<html lang=\"en\" data-theme=\"neutral\">", page, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetHome_Anonymous_RendersLandmarks()
    {
        var page = await GetPageAsync(_homeUri);

        Assert.Contains("<header", page, StringComparison.Ordinal);
        Assert.Contains("<main id=\"main\"", page, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetHome_Anonymous_RendersResourceStrings()
    {
        var page = await GetPageAsync(_homeUri);

        Assert.Contains("Skip to content", page, StringComparison.Ordinal);
        Assert.Contains("Track 100% completion in your games.", page, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetHome_Anonymous_IsStaticWithBlazorScript()
    {
        var page = await GetPageAsync(_homeUri);

        // Interactive components leave "<!--Blazor:" markers in the HTML. Static SSR leaves none.
        Assert.DoesNotContain("<!--Blazor:", page, StringComparison.Ordinal);
        Assert.Contains("_framework/blazor.web", page, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetError_Anonymous_SkipLinkStaysOnPage()
    {
        var page = await GetPageAsync(_errorUri);

        Assert.Contains("/Error#main\"", page, StringComparison.Ordinal);
    }

    private static async Task<string> GetPageAsync(Uri uri)
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(host => host
            .UseSetting("Backend:BaseUrl", "http://backend:8080")
            .UseSetting("ServiceAuth:PrivateKey", Convert.ToBase64String(key.ExportPkcs8PrivateKey())));
        using var client = factory.CreateClient();

        return await client.GetStringAsync(uri);
    }
}
