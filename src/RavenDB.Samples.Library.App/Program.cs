using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/ravendb?tabs=dotnet-cli#client-integration
builder.AddRavenDBClient("library",
    settings =>
    {
        settings.CreateDatabase = true;
    });

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();