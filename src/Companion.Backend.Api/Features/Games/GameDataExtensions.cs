namespace Companion.Backend.Api.Features.Games;

public static class GameDataExtensions
{
    public static IServiceCollection AddGameData(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<GameDataOptions>().Bind(configuration.GetSection(GameDataOptions.SectionName));

        services.AddSingleton<GameCatalog>();
        services.AddHostedService(provider => provider.GetRequiredService<GameCatalog>());

        return services;
    }
}
