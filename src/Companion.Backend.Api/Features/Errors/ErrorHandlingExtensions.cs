namespace Companion.Backend.Api.Features.Errors;

public static class ErrorHandlingExtensions
{
    public static IServiceCollection AddErrorHandling(this IServiceCollection services) =>
        services.AddProblemDetails();

    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app) =>
        app.UseExceptionHandler();
}
