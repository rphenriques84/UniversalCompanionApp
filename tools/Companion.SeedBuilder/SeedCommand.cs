namespace Companion.SeedBuilder;

public static class SeedCommand
{
    public const int Success = 0;
    public const int InvalidData = 1;
    public const int InvalidUsage = 2;

    public static async Task<int> RunAsync(string[] args, TextWriter output, TextWriter error, CancellationToken cancellationToken)
    {
        if (args is not ["validate", var dataDirectory])
        {
            await error.WriteLineAsync("Usage: validate <data-directory>");
            return InvalidUsage;
        }

        var errors = await SeedValidator.ValidateAsync(dataDirectory, cancellationToken);

        foreach (var seedError in errors)
        {
            await error.WriteLineAsync(seedError.ToString());
        }

        await output.WriteLineAsync($"{errors.Count} error(s).");
        return errors.Count == 0 ? Success : InvalidData;
    }
}
