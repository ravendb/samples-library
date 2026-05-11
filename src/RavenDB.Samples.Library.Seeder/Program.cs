using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raven.Migrations;
using RavenDB.Samples.Library.Seeder;
using RavenDB.Samples.Library.Seeder.Migrations;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

builder.AddRavenDBClient("library", settings => settings.CreateDatabase = true);

var queues = builder.Configuration.GetConnectionString("queues")
    ?? throw new InvalidOperationException("ConnectionStrings:queues missing.");
builder.Services.AddSingleton(new MigrationContext(queues));

builder.Services.AddRavenDbMigrations(m =>
    m.Assemblies = [typeof(ImportGoodBooks).Assembly]);

builder.Services.AddSingleton<SampleDataSeeder>();
builder.Services.AddHostedService<SeedAndExitWorker>();

await builder.Build().RunAsync();
