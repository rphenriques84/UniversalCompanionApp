using Companion.Backend.Api.Features.Errors;
using Companion.Backend.Api.Features.Games;
using Companion.Backend.Api.Features.ServiceAuth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddErrorHandling();
builder.Services.AddServiceAuth(builder.Configuration);
builder.Services.AddGameData(builder.Configuration);

var app = builder.Build();

app.UseErrorHandling();
app.UseAuthentication();
app.UseAuthorization();

app.MapGamesEndpoints();

await app.RunAsync();

public partial class Program;
