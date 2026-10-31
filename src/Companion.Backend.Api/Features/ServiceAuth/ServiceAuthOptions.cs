namespace Companion.Backend.Api.Features.ServiceAuth;

public sealed class ServiceAuthOptions
{
    public const string SectionName = "ServiceAuth";

    /// <summary>Base64 SubjectPublicKeyInfo ES256 public key.</summary>
    public string CurrentPublicKey { get; set; } = string.Empty;

    /// <summary>Optional. Keeps tokens from the old key valid during key rotation.</summary>
    public string? PreviousPublicKey { get; set; }
}
