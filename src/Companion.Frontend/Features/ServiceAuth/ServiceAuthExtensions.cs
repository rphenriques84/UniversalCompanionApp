using Microsoft.Extensions.Options;

namespace Companion.Frontend.Features.ServiceAuth;

public static class ServiceAuthExtensions
{
    public static IServiceCollection AddServiceTokens(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ServiceTokenOptions>()
            .Bind(configuration.GetSection(ServiceTokenOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<ServiceTokenOptions>, ServiceTokenOptionsValidator>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ServiceTokenIssuer>();
        services.AddTransient<ServiceTokenHandler>();

        return services;
    }
}
