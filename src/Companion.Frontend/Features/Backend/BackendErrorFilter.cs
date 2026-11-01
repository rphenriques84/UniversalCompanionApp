using System.Globalization;
using Polly;

namespace Companion.Frontend.Features.Backend;

public sealed partial class BackendErrorFilter(ILogger<BackendErrorFilter> logger) : IEndpointFilter
{
    private const string _title = "The service is temporarily unavailable.";
    private const string _noStatus = "none";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            return await next(context);
        }
        catch (Exception exception) when (exception is HttpRequestException or ExecutionRejectedException)
        {
            LogBackendCallFailed(logger, exception.GetType().Name, GetStatus(exception));
            return TypedResults.Problem(title: _title, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    private static string GetStatus(Exception exception) =>
        exception is HttpRequestException { StatusCode: { } status }
            ? ((int)status).ToString(CultureInfo.InvariantCulture)
            : _noStatus;

    [LoggerMessage(Level = LogLevel.Warning, Message = "Backend call failed with {ExceptionType}, status {Status}")]
    private static partial void LogBackendCallFailed(ILogger logger, string exceptionType, string status);
}
