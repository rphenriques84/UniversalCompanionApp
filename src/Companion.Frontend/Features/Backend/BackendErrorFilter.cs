using Polly;

namespace Companion.Frontend.Features.Backend;

public sealed partial class BackendErrorFilter(ILogger<BackendErrorFilter> logger) : IEndpointFilter
{
    private const string _title = "The service is temporarily unavailable.";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            return await next(context);
        }
        catch (Exception exception) when (exception is HttpRequestException or ExecutionRejectedException)
        {
            LogBackendCallFailed(logger, exception.GetType().Name);
            return TypedResults.Problem(title: _title, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Backend call failed with {ExceptionType}")]
    private static partial void LogBackendCallFailed(ILogger logger, string exceptionType);
}
