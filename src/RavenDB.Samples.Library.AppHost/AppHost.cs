using CommunityToolkit.Aspire.Hosting.RavenDB;

var builder = DistributedApplication.CreateBuilder(args);

// RavenDB
// https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/ravendb?tabs=dotnet-cli#hosting-integration
var ravenDbServer = builder
    .AddRavenDB("server")
    .WithDataVolume("ravendb-samples-library");

var db = ravenDbServer
    .AddDatabase("library");
 
// Migrations
var migrations = builder.AddProject<Projects.RavenDB_Samples_Library_Migrations>("migrations")
    .WithReference(db)
    .WaitFor(db);

// Storage
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator();
var queues = storage.AddQueues("queues");

// Catalogue
builder.AddAzureFunctionsProject<Projects.RavenDB_Samples_Library_App>("app")
    .WithHostStorage(storage)
    .WithReference(queues)
    .WithReference(db)
    .WaitFor(db)
    .WaitForCompletion(migrations);

builder.Build().Run();
