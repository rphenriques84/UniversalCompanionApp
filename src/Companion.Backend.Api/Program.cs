using Companion.Backend.Api.Features.ApiDocs;
using Companion.Backend.Api.Features.Errors;
using Companion.Backend.Api.Features.Games;
using Companion.Backend.Api.Features.Observability;
using Companion.Backend.Api.Features.ServiceAuth;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddLocalSecrets(builder.Environment.ContentRootPath);
}

builder.Services.AddObservability(builder.Configuration);
builder.Services.AddErrorHandling();
builder.Services.AddServiceAuth(builder.Configuration);
builder.Services.AddGameData(builder.Configuration);
builder.Services.AddApiDocs();

var app = builder.Build();

app.UseErrorHandling();
app.UseAuthentication();
app.UseAuthorization();

app.MapApiDocsInDevelopment();
app.MapGamesEndpoints();

await app.RunAsync();

public partial class Program;
