namespace Companion.Frontend.Features.ServiceAuth;

public sealed class ServiceTokenOptions
{
    public const string SectionName = "ServiceAuth";

    public static readonly TimeSpan MaxTokenLifetime = TimeSpan.FromMinutes(5);

    /// <summary>Base64 PKCS#8 ES256 private key.</summary>
    public string PrivateKey { get; set; } = string.Empty;

    public TimeSpan TokenLifetime { get; set; } = MaxTokenLifetime;
}
