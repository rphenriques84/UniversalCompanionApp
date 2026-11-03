using Companion.Frontend.Features.ServiceAuth;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

namespace Companion.Frontend.Features.Backend;

public static class BackendExtensions
{
    private const int _maxRetries = 2;

    private static readonly TimeSpan _attemptTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan _totalTimeout = TimeSpan.FromSeconds(15);

    public static IHttpClientBuilder AddBackendClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<BackendOptions>()
            .Bind(configuration.GetSection(BackendOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var client = services.AddHttpClient<BackendClient>((provider, client) =>
            client.BaseAddress = new Uri(provider.GetRequiredService<IOptions<BackendOptions>>().Value.BaseUrl));

        // The default HttpClient logs redact headers in the message only. At Trace level, their
        // structured values still hold the raw Authorization header, so they would leak the service token.
        client.RemoveAllLoggers();

        // Resilience goes first so it wraps the token handler: each attempt gets a current token.
        client.AddStandardResilienceHandler(ConfigureResilience);

        return client.AddHttpMessageHandler<ServiceTokenHandler>();
    }

    private static void ConfigureResilience(HttpStandardResilienceOptions options)
    {
        options.AttemptTimeout.Timeout = _attemptTimeout;
        options.TotalRequestTimeout.Timeout = _totalTimeout;
        options.Retry.MaxRetryAttempts = _maxRetries;
        options.Retry.DisableForUnsafeHttpMethods();
    }
}
