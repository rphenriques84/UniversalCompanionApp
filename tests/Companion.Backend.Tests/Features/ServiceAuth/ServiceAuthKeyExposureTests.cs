using Companion.Backend.Api.Features.ServiceAuth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([KeyValuePair.Create<string, string?>("ServiceAuth:CurrentPublicKey", _badKey)])
            .Build();
        using var provider = new ServiceCollection().AddServiceAuth(configuration).BuildServiceProvider();

        // Runs the startup check directly. Through WebApplicationFactory, a failed start can race with host disposal.
        var exception = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IStartupValidator>().Validate());

        Assert.DoesNotContain(_badKey, exception.ToString(), StringComparison.Ordinal);
    }
}
