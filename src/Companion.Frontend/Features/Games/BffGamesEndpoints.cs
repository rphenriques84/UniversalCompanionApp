using Companion.Contracts;
using Companion.Frontend.Features.Backend;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Companion.Frontend.Features.Games;

public static class BffGamesEndpoints
{
    public static IEndpointRouteBuilder MapBffGamesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var games = endpoints.MapGroup("/bff/games");

        games.MapGet("/", GetGamesAsync);

        return endpoints;
    }

    private static async Task<Ok<IReadOnlyList<GameDto>>> GetGamesAsync(BackendClient backend, CancellationToken cancellationToken) =>
        TypedResults.Ok(await backend.GetGamesAsync(cancellationToken));
}
