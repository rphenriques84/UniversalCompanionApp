// Adds an ES256 key pair for service tokens to .env. Run: dotnet tools/generate-service-keys.cs
// Writes the file itself, because shell redirects use UTF-16 in Windows PowerShell 5.1.
// Never prints the keys, and never commit them.
using System.Security.Cryptography;
using System.Text;

const string PrivateKeyName = "SERVICE_TOKEN_PRIVATE_KEY";
const string PublicKeyName = "SERVICE_TOKEN_PUBLIC_KEY";

var path = args.Length > 0 ? args[0] : ".env";

if (!File.Exists(path))
{
    Console.Error.WriteLine($"{path} not found. Copy .env.example to .env first.");
    return 1;
}

var content = File.ReadAllText(path);

if (content.Split('\n').Any(line => line.StartsWith($"{PrivateKeyName}=", StringComparison.Ordinal)))
{
    Console.Error.WriteLine($"{path} already has keys. Nothing changed.");
    return 1;
}

using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);

var separator = content.Length == 0 || content.EndsWith('\n') ? string.Empty : "\n";
var keys = $"{PrivateKeyName}={Convert.ToBase64String(key.ExportPkcs8PrivateKey())}\n"
    + $"{PublicKeyName}={Convert.ToBase64String(key.ExportSubjectPublicKeyInfo())}\n";

File.WriteAllText(path, content + separator + keys, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
Console.WriteLine($"Keys added to {path}.");
return 0;
