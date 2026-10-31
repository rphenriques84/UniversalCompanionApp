using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace Companion.Backend.Api.Features.ServiceAuth;

public sealed class ServiceAuthOptionsValidator : IValidateOptions<ServiceAuthOptions>
{
    public ValidateOptionsResult Validate(string? name, ServiceAuthOptions options)
    {
        if (!IsValidPublicKey(options.CurrentPublicKey))
        {
            return ValidateOptionsResult.Fail("ServiceAuth:CurrentPublicKey must be a Base64 P-256 public key.");
        }

        return string.IsNullOrEmpty(options.PreviousPublicKey) || IsValidPublicKey(options.PreviousPublicKey)
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail("ServiceAuth:PreviousPublicKey must be a Base64 P-256 public key.");
    }

    private static bool IsValidPublicKey(string publicKey)
    {
        try
        {
            using var key = ECDsa.Create();
            key.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);
            return key.KeySize == 256;
        }
        catch (Exception exception) when (exception is FormatException or CryptographicException)
        {
            return false;
        }
    }
}
