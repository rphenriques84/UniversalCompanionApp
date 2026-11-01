namespace Companion.SeedBuilder.Tests;

/// <summary>A temporary data folder that uses the real schemas from data/schema.</summary>
internal sealed class SeedDataFolder : IDisposable
{
    public SeedDataFolder()
    {
        Path = Directory.CreateTempSubdirectory("seed-tests").FullName;
        var schemaFolder = Directory.CreateDirectory(System.IO.Path.Combine(Path, "schema"));

        foreach (var schema in Directory.EnumerateFiles(FindRealSchemaFolder()))
        {
            File.Copy(schema, System.IO.Path.Combine(schemaFolder.FullName, System.IO.Path.GetFileName(schema)));
        }
    }

    public string Path { get; }

    public void AddGameFile(string name, string content)
    {
        var folder = Directory.CreateDirectory(System.IO.Path.Combine(Path, "games", "ffx"));
        File.WriteAllText(System.IO.Path.Combine(folder.FullName, name), content);
    }

    public void Dispose() => Directory.Delete(Path, recursive: true);

    private static string FindRealSchemaFolder()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(System.IO.Path.Combine(directory.FullName, "Companion.slnx")))
        {
            directory = directory.Parent;
        }

        return directory is null
            ? throw new InvalidOperationException("Repository root not found.")
            : System.IO.Path.Combine(directory.FullName, "data", "schema");
    }
}
