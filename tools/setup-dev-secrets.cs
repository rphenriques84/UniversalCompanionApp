// Stores a throwaway ES256 key pair in user secrets, for F5 debugging. Run: dotnet tools/setup-dev-secrets.cs
// The frontend gets the private key and the backend the public key. Running it again replaces both.
// Sends the keys over stdin and never prints them.
using System.Diagnostics;
using System.Security.Cryptography;

using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);

var privateKey = Convert.ToBase64String(key.ExportPkcs8PrivateKey());
var publicKey = Convert.ToBase64String(key.ExportSubjectPublicKeyInfo());

return await SetSecretAsync("src/Companion.Frontend", "ServiceAuth:PrivateKey", privateKey)
    && await SetSecretAsync("src/Companion.Backend.Api", "ServiceAuth:CurrentPublicKey", publicKey) ? 0 : 1;

static async Task<bool> SetSecretAsync(string project, string name, string value)
{
    var start = new ProcessStartInfo("dotnet", ["user-secrets", "set", "--project", project])
    {
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
    };

    using var process = Process.Start(start)!;
    await process.StandardInput.WriteAsync($"{{\"{name}\":\"{value}\"}}");
    process.StandardInput.Close();
    var outputTask = process.StandardOutput.ReadToEndAsync();
    var errorTask = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    await Task.WhenAll(outputTask, errorTask);

    if (process.ExitCode != 0)
    {
        Console.Error.WriteLine($"Could not set {name} for {project}.");
        return false;
    }

    Console.WriteLine($"Set {name} for {project}.");
    return true;
}
