using System.Text.RegularExpressions;

namespace Companion.Architecture.Tests;

/// <summary>Static SSR by default. Interactive pages use WebAssembly only, from the client project (ADR 0002).</summary>
public sealed partial class RenderModeRulesTests
{
    private const string _serverProject = "Companion.Frontend";
    private const string _clientProject = "Companion.Frontend.Client";

    [Fact]
    public void FrontendSources_UseNoServerOrAutoRenderMode()
    {
        var offenders = FindFiles(_serverProject, "*.razor", "*.cs")
            .Concat(FindFiles(_clientProject, "*.razor", "*.cs"))
            .Where(file => ServerOrAutoRenderMode().IsMatch(File.ReadAllText(file)))
            .ToList();

        Assert.Empty(offenders);
    }

    [Fact]
    public void ServerComponents_SetNoRenderMode()
    {
        // A render mode here would make a page, or the whole app, interactive.
        var offenders = FindFiles(_serverProject, "*.razor")
            .Where(file => File.ReadAllText(file).Contains("@rendermode", StringComparison.Ordinal))
            .ToList();

        Assert.Empty(offenders);
    }

    [Fact]
    public void ClientPages_UseInteractiveWebAssembly()
    {
        var offenders = FindFiles(_clientProject, "*.razor")
            .Select(File.ReadAllText)
            .Where(content => content.Contains("@page", StringComparison.Ordinal))
            .Where(content => !content.Contains("@rendermode InteractiveWebAssembly", StringComparison.Ordinal))
            .ToList();

        Assert.Empty(offenders);
    }

    private static IEnumerable<string> FindFiles(string project, params string[] patterns) =>
        patterns
            .SelectMany(pattern => Directory.GetFiles(Path.Combine(ProjectFile.FindRepositoryRoot(), "src", project), pattern, SearchOption.AllDirectories))
            .Where(file => !file.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(folder => folder is "bin" or "obj"));

    [GeneratedRegex(@"InteractiveServer|InteractiveAuto")]
    private static partial Regex ServerOrAutoRenderMode();
}
