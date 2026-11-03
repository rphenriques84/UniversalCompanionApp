using Microsoft.Playwright;

namespace Companion.E2E;

public sealed class ClientCheckTests
{
    private const string _clientCheckUrl = "http://127.0.0.1:8080/client-check";

    [E2EFact]
    public async Task OpenClientCheck_RealBrowser_WebAssemblyRuns()
    {
        // Downloads Chromium once into the Playwright cache. Later runs reuse it.
        Assert.Equal(0, Program.Main(["install", "chromium"]));
        await using var stack = await ComposeStack.StartAsync();
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync(_clientCheckUrl);

        await Assertions.Expect(page.GetByRole(AriaRole.Status)).ToHaveTextAsync("Interactive mode is running in your browser.");
    }
}
