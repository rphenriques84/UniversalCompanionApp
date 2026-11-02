using Companion.Frontend.Components;
using Companion.Frontend.Features.ApiDocs;
using Companion.Frontend.Features.Backend;
using Companion.Frontend.Features.Games;
using Companion.Frontend.Features.ServiceAuth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceTokens(builder.Configuration);
builder.Services.AddBackendClient(builder.Configuration);
builder.Services.AddOpenApi();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapApiDocsInDevelopment();
app.MapBffGamesEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Companion.Frontend.Client._Imports).Assembly);

await app.RunAsync();

public partial class Program;
