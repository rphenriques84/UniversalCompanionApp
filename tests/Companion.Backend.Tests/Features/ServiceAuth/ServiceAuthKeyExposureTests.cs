using Companion.Backend.Api.Features.ServiceAuth;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Options;

namespace Companion.Backend.Tests.Features.ServiceAuth;

public sealed class ServiceAuthKeyExposureTests
{
    private const string _badKey = "bad-key-value-that-must-stay-hidden";

    [Fact]
    public void Validate_InvalidCurrentKey_OmitsKeyValue()
    {
        var result = new ServiceAuthOptionsValidator().Validate(null, new ServiceAuthOptions { CurrentPublicKey = _badKey });

        Assert.True(result.Failed);
        Assert.DoesNotContain(_badKey, result.FailureMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void Startup_InvalidKey_OmitsKeyValueFromError()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(host => host.UseSetting("ServiceAuth:CurrentPublicKey", _badKey));

        var exception = Assert.Throws<OptionsValidationException>(() => factory.CreateClient());

        Assert.DoesNotContain(_badKey, exception.ToString(), StringComparison.Ordinal);
    }
}
