using Companion.Frontend.Features.ServiceAuth;
using Microsoft.Extensions.Options;

namespace Companion.Frontend.Features.Backend;

public static class BackendExtensions
{
    public static IHttpClientBuilder AddBackendClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<BackendOptions>()
            .Bind(configuration.GetSection(BackendOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services
            .AddHttpClient<BackendClient>((provider, client) =>
                client.BaseAddress = new Uri(provider.GetRequiredService<IOptions<BackendOptions>>().Value.BaseUrl))
            .AddHttpMessageHandler<ServiceTokenHandler>();
    }
}
