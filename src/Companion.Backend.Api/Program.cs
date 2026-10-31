using Companion.Backend.Api.Features.ServiceAuth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceAuth(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

await app.RunAsync();
