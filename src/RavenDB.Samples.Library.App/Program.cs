using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raven.Migrations;
using RavenDB.Samples.Library.Setup.Migrations;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Services
    .AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

builder.ConfigureFunctionsWebApplication();

// https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/ravendb?tabs=dotnet-cli#client-integration
builder.AddRavenDBClient("library",
    settings =>
    {
        settings.CreateDatabase = true;
    });

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddRavenDbMigrations(migrations =>
    {
        migrations.Assemblies = [typeof(ImportGoodBooks).Assembly];
    });

builder.Build().Run();