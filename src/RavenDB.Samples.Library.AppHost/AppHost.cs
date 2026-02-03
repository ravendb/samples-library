using CommunityToolkit.Aspire.Hosting.RavenDB;

var builder = DistributedApplication.CreateBuilder(args);

// Storage
var storage = builder.AddAzureStorage("storage").RunAsEmulator();
var queues = storage.AddQueues("queues");

// RavenDB
// https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/ravendb?tabs=dotnet-cli#hosting-integration
var license = Environment.GetEnvironmentVariable("RAVEN_LICENSE");

var settings = RavenDBServerSettings.Unsecured();
if (license != null)
{
    // Use the license from the environmental variable
    settings.WithLicense(license);
}

settings.Port = 9534;
settings.TcpPort = 41350;

var ravenDbServer = builder
    .AddRavenDB("RavenDB", settings)
    .WithImage("ravendb/ravendb", "7.1-latest")
    .WithIconName("Database")
    .WithReference(queues)
    .WaitFor(queues);

const string dbName = "library";

var db = ravenDbServer
    .AddDatabase(dbName);

const string commandKey = "CommandKey";

var secretKey = builder.AddParameter(commandKey, secret: true);

// Library App
var functions = builder.AddAzureFunctionsProject<Projects.RavenDB_Samples_Library_App>("app")
    .WithHostStorage(storage)
    
    .WithReference(queues)
    .WaitFor(queues)
    
    // Required to go separately with as the bindings are not wired with the host storage
    .WithEnvironment("BindingConnection", queues.Resource.ConnectionStringExpression)
    
    .WithReference(db)
    .WaitFor(db)
    
    .WithEnvironment(commandKey, secretKey)
    .WithHttpCommand(
        path: "/api/migrate",
        displayName: "Migrate DB",

        commandOptions: new HttpCommandOptions
        {
            Description = "Runs database migrations",
            PrepareRequest = async context =>
            {
                var key = await secretKey.Resource.GetValueAsync(context.CancellationToken);
                context.Request.Headers.Add("X-Command-Key", key);
            },
            IconName = "databaseArrowUp",
            IsHighlighted = true
        });

// Frontend
var frontend = builder.AddNpmApp("Frontend", "../RavenDB.Samples.Library.Frontend", "dev")
    .WithReference(functions)
    .WithEnvironment("BROWSER", "none")
    .WithEnvironment("APP_HTTP", functions.GetEndpoint("http"))
    .WithHttpEndpoint(env: "VITE_PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile()
    .WaitFor(functions);    

builder.Build().Run();