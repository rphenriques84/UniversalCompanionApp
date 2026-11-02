using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace Companion.Backend.Api.Features.ApiDocs;

public static class ApiDocsExtensions
{
    private const string _bearerScheme = "Bearer";

    public static IServiceCollection AddApiDocs(this IServiceCollection services) =>
        services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes[_bearerScheme] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
            };
            document.Security = [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference(_bearerScheme, document)] = [] }];

            return Task.CompletedTask;
        }));

    public static void MapApiDocsInDevelopment(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        // Only the docs are open. Every API call from the page still needs a service token.
        app.MapOpenApi().AllowAnonymous();
        app.MapScalarApiReference().AllowAnonymous();
    }
}
