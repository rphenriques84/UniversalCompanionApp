using Microsoft.Extensions.Configuration.Memory;

namespace Companion.Backend.Api.Features.ServiceAuth;

public static class LocalSecretsExtensions
{
    private const string _fileName = ".env";

    // Only what this service needs. The frontend keeps its own list.
    private static readonly Dictionary<string, string> _settingNames = new(StringComparer.Ordinal)
    {
        ["SERVICE_TOKEN_PUBLIC_KEY"] = $"{ServiceAuthOptions.SectionName}:{nameof(ServiceAuthOptions.CurrentPublicKey)}",
        ["SERVICE_TOKEN_PREVIOUS_PUBLIC_KEY"] = $"{ServiceAuthOptions.SectionName}:{nameof(ServiceAuthOptions.PreviousPublicKey)}",
    };

    /// <summary>Loads secrets from the nearest .env file, for F5 debugging. Every other configuration source wins.</summary>
    public static void AddLocalSecrets(this IConfigurationBuilder configuration, string startDirectory)
    {
        for (var directory = new DirectoryInfo(startDirectory); directory is not null; directory = directory.Parent)
        {
            var path = Path.Combine(directory.FullName, _fileName);

            if (File.Exists(path))
            {
                configuration.Sources.Insert(0, new MemoryConfigurationSource { InitialData = Read(path) });
                return;
            }
        }
    }

    private static List<KeyValuePair<string, string?>> Read(string path) =>
    [
        .. File.ReadLines(path)
            .Select(line => line.Split('=', 2))
            .Where(parts => parts.Length == 2 && _settingNames.ContainsKey(parts[0].Trim()))
            .Select(parts => KeyValuePair.Create(_settingNames[parts[0].Trim()], (string?)parts[1].Trim())),
    ];
}
