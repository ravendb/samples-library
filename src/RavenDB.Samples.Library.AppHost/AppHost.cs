var builder = DistributedApplication.CreateBuilder(args);


// RavenDB
// https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/ravendb?tabs=dotnet-cli#hosting-integration
var ravenDbServer = builder
    .AddRavenDB("RavenDB")
    .WithIconName("Database");

const string dbName = "library";

var db = ravenDbServer
    .AddDatabase(dbName);

// Storage
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator();
var queues = storage.AddQueues("queues");

var secretKey = builder.AddParameter("CommandKey", secret: true);

// Library App
builder.AddAzureFunctionsProject<Projects.RavenDB_Samples_Library_App>("app")
    .WithHostStorage(storage)
    .WithReference(queues)
    .WithReference(db)
    .WaitFor(db)
    .WithEnvironment("CommandKey", secretKey)
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

builder.Build().Run();