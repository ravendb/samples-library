var builder = DistributedApplication.CreateBuilder(args);

// RavenDB
// https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/ravendb?tabs=dotnet-cli#hosting-integration
var ravenDbServer = builder
    .AddRavenDB("RavenDB")
    .WithDataVolume("ravendb-samples-library");

var db = ravenDbServer
    .AddDatabase("library");
 
// Setup project, including migrations
var setup = builder.AddProject<Projects.RavenDB_Samples_Library_Setup>("setup")
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
    .WaitForCompletion(setup);

builder.Build().Run();
