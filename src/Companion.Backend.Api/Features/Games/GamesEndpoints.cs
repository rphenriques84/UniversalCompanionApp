using Companion.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Companion.Backend.Api.Features.Games;

public static class GamesEndpoints
{
    public static IEndpointRouteBuilder MapGamesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var games = endpoints.MapGroup("/games").RequireAuthorization(ServiceScopes.GameDataRead);

        games.MapGet("/", GetGames);

        return endpoints;
    }

    private static Ok<IEnumerable<GameDto>> GetGames(GameCatalog catalog) =>
        TypedResults.Ok(catalog.Games.Select(game => new GameDto(game.Key, game.Name)));
}
