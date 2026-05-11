using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

// Enable CORS for local development with dynamically allocated Aspire ports.
// For production deployments, configure specific allowed origins.
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
builder.AddRavenDBClient("library");

builder.Services
    .AddOpenTelemetry().WithLogging();

builder.Build().Run();