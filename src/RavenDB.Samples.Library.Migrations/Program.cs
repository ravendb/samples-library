using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raven.Migrations;

var builder = Host.CreateApplicationBuilder();

// https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/ravendb?tabs=dotnet-cli#client-integration
builder.AddRavenDBClient("library",
    settings =>
    {
        settings.CreateDatabase = true;
    });

builder.Services.AddRavenDbMigrations();
builder.Services.AddHostedService<MigrationService>();

await builder.Build().RunAsync();