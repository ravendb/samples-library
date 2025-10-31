using CommunityToolkit.Aspire.Hosting.RavenDB;

var builder = DistributedApplication.CreateBuilder(args);

// RavenDB
var ravenDbServer = builder
    .AddRavenDB("server")
    .WithDataVolume("ravendb-samples-library");

var db = ravenDbServer
    .AddDatabase("library");

// Storage
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator();
var queues = storage.AddQueues("queues");

// Catalogue
builder.AddAzureFunctionsProject<Projects.RavenDB_Samples_Library_Catalogue_App>("catalogue")
    .WithHostStorage(storage)
    .WithReference(queues);

builder.Build().Run();
