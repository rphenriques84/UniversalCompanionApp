using static Companion.Architecture.Tests.ProjectNames;

namespace Companion.Architecture.Tests;

public sealed class DependencyRulesTests
{
    private static readonly Dictionary<string, string[]> _allowedReferences = new()
    {
        [Domain] = [],
        [Contracts] = [],
        [FrontendClient] = [Domain, Contracts],
        [Frontend] = [FrontendClient, Contracts, Domain],
        [BackendData] = [Domain],
        [BackendApi] = [BackendData, Contracts],
    };

    private static readonly string[] _databasePackagePrefixes =
    [
        "Microsoft.EntityFrameworkCore",
        "Npgsql",
    ];

    private static readonly string[] _forbiddenPackagePrefixes =
    [
        "Microsoft.AspNetCore",
        .. _databasePackagePrefixes,
    ];

    private readonly IReadOnlyList<ProjectFile> _projects = ProjectFile.LoadSourceProjects();

    public static TheoryData<string> ProjectsWithRules => new(_allowedReferences.Keys);

    [Fact]
    public void SourceProjects_AllHaveDependencyRules()
    {
        var names = _projects.Select(project => project.Name);

        Assert.Equal(_allowedReferences.Keys.Order(), names.Order());
    }

    [Theory]
    [MemberData(nameof(ProjectsWithRules))]
    public void ProjectReferences_MatchDependencyRules(string projectName)
    {
        var project = _projects.Single(candidate => candidate.Name == projectName);

        Assert.Equal(_allowedReferences[projectName].Order(), project.ProjectReferences.Order());
    }

    [Theory]
    [InlineData(Domain)]
    [InlineData(Contracts)]
    public void SharedProjects_HaveNoEfCoreOrAspNetReferences(string projectName)
    {
        var project = _projects.Single(candidate => candidate.Name == projectName);

        Assert.Equal("Microsoft.NET.Sdk", project.Sdk);
        Assert.Empty(project.FrameworkReferences);
        Assert.DoesNotContain(project.PackageReferences, IsForbiddenPackage);
    }

    [Theory]
    [InlineData(Frontend)]
    [InlineData(FrontendClient)]
    public void FrontendProjects_HaveNoDatabaseReferences(string projectName)
    {
        var project = _projects.Single(candidate => candidate.Name == projectName);

        Assert.DoesNotContain(project.PackageReferences, IsDatabasePackage);
    }

    private static bool IsForbiddenPackage(string package) =>
        _forbiddenPackagePrefixes.Any(prefix => package.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

    private static bool IsDatabasePackage(string package) =>
        _databasePackagePrefixes.Any(prefix => package.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
}
