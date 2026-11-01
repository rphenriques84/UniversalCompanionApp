using System.Diagnostics;
using System.Globalization;

namespace Companion.E2E;

/// <summary>
/// Runs the real Compose stack with throwaway keys from a temporary env file.
/// It never uses the real .env, because Compose errors can print its contents.
/// </summary>
internal sealed class ComposeStack : IAsyncDisposable
{
    private const string _projectName = "companion-e2e";
    private const int _startTimeoutSeconds = 240;

    private readonly string _repositoryRoot = FindRepositoryRoot();
    private readonly string _envFile = Path.GetTempFileName();

    private ComposeStack()
    {
    }

    public static async Task<ComposeStack> StartAsync()
    {
        var stack = new ComposeStack();

        try
        {
            File.Copy(Path.Combine(stack._repositoryRoot, ".env.example"), stack._envFile, overwrite: true);
            await stack.RunAsync("dotnet", "tools/generate-service-keys.cs", stack._envFile);
            await stack.RunComposeAsync("up", "--detach", "--build", "--wait", "--wait-timeout", _startTimeoutSeconds.ToString(CultureInfo.InvariantCulture));
            return stack;
        }
        catch
        {
            await stack.DisposeAsync();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await RunComposeAsync("down", "--volumes");
        File.Delete(_envFile);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Companion.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }

    private Task RunComposeAsync(params string[] arguments) =>
        RunAsync("docker", ["compose", "--project-name", _projectName, "--env-file", _envFile, .. arguments]);

    private async Task RunAsync(string fileName, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo(fileName)
        {
            WorkingDirectory = _repositoryRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"{fileName} failed: {await output}{await error}");
        }
    }
}
