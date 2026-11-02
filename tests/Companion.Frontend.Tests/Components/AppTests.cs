using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Companion.Frontend.Tests.Components;

public sealed class AppTests
{
    private static readonly Uri _homeUri = new("/", UriKind.Relative);

    [Fact]
    public async Task GetHome_Anonymous_UsesNeutralTheme()
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(host => host
            .UseSetting("Backend:BaseUrl", "http://backend:8080")
            .UseSetting("ServiceAuth:PrivateKey", Convert.ToBase64String(key.ExportPkcs8PrivateKey())));
        using var client = factory.CreateClient();

        var page = await client.GetStringAsync(_homeUri);

        Assert.Contains("<html lang=\"en\" data-theme=\"neutral\">", page, StringComparison.Ordinal);
    }
}
