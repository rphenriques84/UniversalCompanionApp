using System.Text.RegularExpressions;

namespace Companion.Architecture.Tests;

public sealed partial class CodeStyleRulesTests
{
    private static readonly string[] _sourceFolders = ["src", "tests", "tools"];

    [Fact]
    public void SourceFiles_HaveOneTopLevelType()
    {
        var offenders = _sourceFolders
            .SelectMany(FindSourceFiles)
            .Where(file => TopLevelType().Count(File.ReadAllText(file)) > 1)
            .ToList();

        Assert.Empty(offenders);
    }

    private static IEnumerable<string> FindSourceFiles(string folder) =>
        Directory.GetFiles(Path.Combine(ProjectFile.FindRepositoryRoot(), folder), "*.cs", SearchOption.AllDirectories)
            .Where(file => !IsBuildOutput(file));

    private static bool IsBuildOutput(string file)
    {
        var separators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        var folders = file.Split(separators);

        return folders.Contains("bin") || folders.Contains("obj");
    }

    // File-scoped namespaces keep top-level types at column 0, and nested types are indented.
    [GeneratedRegex(@"^(?:(?:public|internal|static|sealed|abstract|partial|readonly)\s+)*(?:class|record|struct|interface|enum)\s", RegexOptions.Multiline)]
    private static partial Regex TopLevelType();
}
