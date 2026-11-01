using Companion.Contracts;
using Companion.Frontend.Features.ServiceAuth;

namespace Companion.Frontend.Features.Backend;

public sealed class BackendClient(HttpClient httpClient)
{
    private static readonly Uri _gamesUri = new("games", UriKind.Relative);

    public async Task<IReadOnlyList<GameDto>> GetGamesAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, _gamesUri);
        request.Options.Set(ServiceTokenHandler.Scope, ServiceScopes.GameDataRead);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var games = await response.Content.ReadFromJsonAsync<GameDto[]>(cancellationToken);
        return games ?? [];
    }
}
