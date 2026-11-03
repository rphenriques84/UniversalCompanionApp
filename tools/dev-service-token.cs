// Prints a 5-minute service token for calling the backend while debugging. Run: dotnet tools/dev-service-token.cs [scope]
// Signs with the private key in .env, so only a backend using the matching public key accepts it.
#:project ../src/Companion.Contracts/Companion.Contracts.csproj
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Companion.Contracts;

const string PrivateKeyPrefix = "SERVICE_TOKEN_PRIVATE_KEY=";
const string SecretsFile = ".env";

var scope = args.Length > 0 ? args[0] : ServiceScopes.GameDataRead;
var privateKey = File.Exists(SecretsFile)
    ? File.ReadLines(SecretsFile).FirstOrDefault(line => line.StartsWith(PrivateKeyPrefix, StringComparison.Ordinal))?[PrivateKeyPrefix.Length..].Trim()
    : null;

if (string.IsNullOrEmpty(privateKey))
{
    Console.Error.WriteLine("No key found in .env. Run: dotnet tools/generate-service-keys.cs");
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
