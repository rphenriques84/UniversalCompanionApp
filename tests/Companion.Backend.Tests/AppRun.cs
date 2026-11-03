using System.Diagnostics;

namespace Companion.Backend.Tests;

/// <summary>Runs the compiled backend as a real process, with all logging on, and captures everything it prints.</summary>
internal sealed record AppRun(int ExitCode, string Output)
{
    private const string _project = "Companion.Backend.Api";
    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(60);

    public static async Task<AppRun> StartAsync(IReadOnlyDictionary<string, string> settings)
    {
        // The test output is bin/<configuration>/<framework>. The app was built with the same pair.
        var testOutput = new DirectoryInfo(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar));
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = Path.Combine(FindRepositoryRoot(), "src", _project, "bin", testOutput.Parent!.Name, testOutput.Name),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        startInfo.ArgumentList.Add($"{_project}.dll");

        // Production never reads .env. Trace shows every log the app can write.
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
        startInfo.Environment["ASPNETCORE_URLS"] = "http://127.0.0.1:0";
        startInfo.Environment["Logging__LogLevel__Default"] = "Trace";
        startInfo.Environment["Logging__LogLevel__Microsoft.AspNetCore"] = "Trace";

        foreach (var (name, value) in settings)
        {
            startInfo.Environment[name] = value;
        }

        using var process = Process.Start(startInfo)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(_timeout);

        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException("The app kept running when it should have failed to start.");
        }

        return new AppRun(process.ExitCode, await output + await error);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Companion.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Repository root not found.");
    }
}
