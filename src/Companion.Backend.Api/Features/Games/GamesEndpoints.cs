using Companion.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Companion.Backend.Api.Features.Games;

public static class GamesEndpoints
{
    // Fixed until P0.6.12 serves loaded seed data.
    private static readonly IReadOnlyList<GameDto> _games = [new("ffx", "Final Fantasy X HD Remaster")];

    public static IEndpointRouteBuilder MapGamesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var games = endpoints.MapGroup("/games").RequireAuthorization(ServiceScopes.GameDataRead);

        games.MapGet("/", GetGames);

        return endpoints;
    }

    private static Ok<IReadOnlyList<GameDto>> GetGames() => TypedResults.Ok(_games);
}
