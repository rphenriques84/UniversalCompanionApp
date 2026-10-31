using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace Companion.Frontend.Features.ServiceAuth;

public sealed class ServiceTokenOptionsValidator : IValidateOptions<ServiceTokenOptions>
{
    public ValidateOptionsResult Validate(string? name, ServiceTokenOptions options)
    {
        if (options.TokenLifetime <= TimeSpan.Zero || options.TokenLifetime > ServiceTokenOptions.MaxTokenLifetime)
        {
            return ValidateOptionsResult.Fail("ServiceAuth:TokenLifetime must be above 0 and at most 5 minutes.");
        }

        return IsValidPrivateKey(options.PrivateKey)
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail("ServiceAuth:PrivateKey must be a Base64 PKCS#8 P-256 private key.");
    }

    private static bool IsValidPrivateKey(string privateKey)
    {
        try
        {
            using var key = ECDsa.Create();
            key.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);
            return key.KeySize == 256;
        }
        catch (Exception exception) when (exception is FormatException or CryptographicException)
        {
            return false;
        }
    }
}
