using System.Security.Cryptography;
using Companion.Frontend.Features.ServiceAuth;

namespace Companion.Frontend.Tests.Features.ServiceAuth;

public sealed class ServiceTokenOptionsValidatorTests
{
    private readonly ServiceTokenOptionsValidator _validator = new();

    [Fact]
    public void Validate_ValidKey_Succeeds()
    {
        var result = _validator.Validate(null, new ServiceTokenOptions { PrivateKey = CreateKey(ECCurve.NamedCurves.nistP256) });

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-base64!")]
    [InlineData("AAAA")]
    public void Validate_InvalidKey_Fails(string privateKey)
    {
        var result = _validator.Validate(null, new ServiceTokenOptions { PrivateKey = privateKey });

        Assert.True(result.Failed);
    }

    [Fact]
    public void Validate_InvalidKey_OmitsKeyValue()
    {
        const string BadKey = "bad-key-value-that-must-stay-hidden";

        var result = _validator.Validate(null, new ServiceTokenOptions { PrivateKey = BadKey });

        Assert.True(result.Failed);
        Assert.DoesNotContain(BadKey, result.FailureMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_WrongCurve_Fails()
    {
        var result = _validator.Validate(null, new ServiceTokenOptions { PrivateKey = CreateKey(ECCurve.NamedCurves.nistP384) });

        Assert.True(result.Failed);
    }

    [Fact]
    public void Validate_LifetimeOverFiveMinutes_Fails()
    {
        var options = new ServiceTokenOptions
        {
            PrivateKey = CreateKey(ECCurve.NamedCurves.nistP256),
            TokenLifetime = TimeSpan.FromMinutes(6),
        };

        Assert.True(_validator.Validate(null, options).Failed);
    }

    private static string CreateKey(ECCurve curve)
    {
        using var key = ECDsa.Create(curve);
        return Convert.ToBase64String(key.ExportPkcs8PrivateKey());
    }
}
