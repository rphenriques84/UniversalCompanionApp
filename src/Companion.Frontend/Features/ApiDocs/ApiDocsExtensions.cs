using Scalar.AspNetCore;

namespace Companion.Frontend.Features.ApiDocs;

public static class ApiDocsExtensions
{
    public static void MapApiDocsInDevelopment(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        app.MapOpenApi();
        app.MapScalarApiReference();
    }
}
