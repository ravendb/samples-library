using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raven.Migrations;
using RavenDB.Samples.Library.Setup.Migrations;

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
builder.AddRavenDBClient("library",
    settings =>
    {
        settings.CreateDatabase = true;
    });


// TODO: extract from the value under container. As RavenDB is used in a container, the wiring is different from the app.
var connection = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;QueueEndpoint=http://storage:10001/devstoreaccount1;";
builder.Services.AddSingleton(new MigrationContext(connection));

builder.Services
    .AddOpenTelemetry().WithLogging();
builder.Services
    .AddRavenDbMigrations(migrations =>
    {
        migrations.Assemblies = [typeof(ImportGoodBooks).Assembly];
    });

builder.Build().Run();