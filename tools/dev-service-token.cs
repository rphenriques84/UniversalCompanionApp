// Prints a 5-minute service token for calling the backend while debugging. Run: dotnet tools/dev-service-token.cs [scope]
// Signs with the throwaway key from tools/setup-dev-secrets.cs, so only a local backend accepts it.
#:project ../src/Companion.Contracts/Companion.Contracts.csproj
using System.Buffers.Text;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Companion.Contracts;

const string PrivateKeyPrefix = "ServiceAuth:PrivateKey = ";

var scope = args.Length > 0 ? args[0] : ServiceScopes.GameDataRead;
var privateKey = await ReadPrivateKeyAsync();

if (privateKey is null)
{
    Console.Error.WriteLine("No dev key found. Run: dotnet tools/setup-dev-secrets.cs");
    return 1;
}

using var key = ECDsa.Create();
key.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);

var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
var header = """{"alg":"ES256","typ":"JWT"}""";
var payload = $$"""{"iss":"{{ServiceTokens.Issuer}}","aud":"{{ServiceTokens.Audience}}","iat":{{now}},"nbf":{{now}},"exp":{{now + 300}},"{{ServiceTokens.ScopeClaim}}":"{{scope}}"}""";
var content = $"{Encode(header)}.{Encode(payload)}";
var signature = key.SignData(Encoding.ASCII.GetBytes(content), HashAlgorithmName.SHA256);

Console.WriteLine($"{content}.{Base64Url.EncodeToString(signature)}");
return 0;

static string Encode(string json) => Base64Url.EncodeToString(Encoding.UTF8.GetBytes(json));

static async Task<string?> ReadPrivateKeyAsync()
{
    var start = new ProcessStartInfo("dotnet", ["user-secrets", "list", "--project", "src/Companion.Frontend"])
    {
        RedirectStandardOutput = true,
    };

    using var process = Process.Start(start)!;
    var output = await process.StandardOutput.ReadToEndAsync();
    await process.WaitForExitAsync();

    return output.Split('\n')
        .Select(line => line.Trim())
        .FirstOrDefault(line => line.StartsWith(PrivateKeyPrefix, StringComparison.Ordinal))?[PrivateKeyPrefix.Length..];
}
