using System.Net.Http.Headers;

namespace Companion.Frontend.Features.ServiceAuth;

public sealed class ServiceTokenHandler(ServiceTokenIssuer issuer) : DelegatingHandler
{
    public static readonly HttpRequestOptionsKey<string> Scope = new("ServiceToken.Scope");
    public static readonly HttpRequestOptionsKey<string> UserId = new("ServiceToken.UserId");

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Options.TryGetValue(Scope, out var scope))
        {
            throw new InvalidOperationException("Backend requests must set a service token scope.");
        }

        request.Options.TryGetValue(UserId, out var userId);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", issuer.GetToken(scope, userId));

        return base.SendAsync(request, cancellationToken);
    }
}
