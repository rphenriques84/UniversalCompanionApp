// Generates an ES256 key pair for service tokens. Run: dotnet tools/generate-service-keys.cs
// Put the output in .env (git-ignored). Never commit it.
using System.Security.Cryptography;

using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);

Console.WriteLine($"SERVICE_TOKEN_PRIVATE_KEY={Convert.ToBase64String(key.ExportPkcs8PrivateKey())}");
Console.WriteLine($"SERVICE_TOKEN_PUBLIC_KEY={Convert.ToBase64String(key.ExportSubjectPublicKeyInfo())}");
