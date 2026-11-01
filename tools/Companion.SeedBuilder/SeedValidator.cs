using System.Text.Json;
using LateApexEarlySpeed.Json.Schema;
using LateApexEarlySpeed.Json.Schema.Common;

namespace Companion.SeedBuilder;

/// <summary>Checks each file in games/ against the schema that shares its name: game.json uses game.schema.json.</summary>
public static class SeedValidator
{
    private const string _gamesFolder = "games";
    private const string _schemaFolder = "schema";

    private static readonly JsonSchemaOptions _reportAllErrors = new() { OutputFormat = OutputFormat.List };

    public static async Task<IReadOnlyList<SeedError>> ValidateAsync(string dataDirectory, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(dataDirectory))
        {
            return [new SeedError(dataDirectory, "Folder not found.")];
        }

        var gamesDirectory = Path.Combine(dataDirectory, _gamesFolder);

        if (!Directory.Exists(gamesDirectory))
        {
            return [];
        }

        var errors = new List<SeedError>();

        foreach (var file in Directory.EnumerateFiles(gamesDirectory, "*.json", SearchOption.AllDirectories).Order())
        {
            errors.AddRange(await ValidateFileAsync(file, dataDirectory, cancellationToken));
        }

        return errors;
    }

    private static async Task<IReadOnlyList<SeedError>> ValidateFileAsync(string file, string dataDirectory, CancellationToken cancellationToken)
    {
        var schemaFile = Path.Combine(dataDirectory, _schemaFolder, $"{Path.GetFileNameWithoutExtension(file)}.schema.json");

        if (!File.Exists(schemaFile))
        {
            return [new SeedError(file, $"No schema found at {schemaFile}.")];
        }

        var validator = new JsonValidator(await File.ReadAllTextAsync(schemaFile, cancellationToken));

        try
        {
            var result = validator.Validate(await File.ReadAllTextAsync(file, cancellationToken), _reportAllErrors);

            return [.. result.ValidationErrors.Select(error => new SeedError(file, $"{error.InstanceLocation}: {error.ErrorMessage}"))];
        }
        catch (JsonException)
        {
            return [new SeedError(file, "Not valid JSON.")];
        }
    }
}
