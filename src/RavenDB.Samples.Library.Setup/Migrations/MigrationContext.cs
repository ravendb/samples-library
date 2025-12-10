namespace RavenDB.Samples.Library.Setup.Migrations;

/// <summary>
/// The context passed for the migrations.
/// </summary>
public class MigrationContext(string azureStorageQueuesConnectionStringForRavenEtl)
{
    public string AzureStorageQueuesConnectionStringForRavenETL { get; } = azureStorageQueuesConnectionStringForRavenEtl;
}