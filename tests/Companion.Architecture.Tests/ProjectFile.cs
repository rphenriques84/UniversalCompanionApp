using System.Xml.Linq;

namespace Companion.Architecture.Tests;

internal sealed record ProjectFile(
    string Name,
    string Sdk,
    IReadOnlySet<string> ProjectReferences,
    IReadOnlySet<string> PackageReferences,
    IReadOnlySet<string> FrameworkReferences)
{
    public static IReadOnlyList<ProjectFile> LoadSourceProjects() =>
        Directory.GetFiles(Path.Combine(FindRepositoryRoot(), "src"), "*.csproj", SearchOption.AllDirectories)
            .Select(Load)
            .ToList();

    private static ProjectFile Load(string path)
    {
        var project = XDocument.Load(path).Root!;

        return new ProjectFile(
            Path.GetFileNameWithoutExtension(path),
            project.Attribute("Sdk")!.Value,
            Includes(project, "ProjectReference", ToProjectName),
            Includes(project, "PackageReference", include => include),
            Includes(project, "FrameworkReference", include => include));
    }

    private static HashSet<string> Includes(XElement project, string itemType, Func<string, string> map) =>
        project.Descendants(itemType)
            .Select(item => map(item.Attribute("Include")!.Value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static string ToProjectName(string include) =>
        Path.GetFileNameWithoutExtension(include.Replace('\\', '/'));

    public static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Companion.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
