using Raven.Client.Documents.Operations.ConnectionStrings;
using Raven.Client.Documents.Operations.ETL;
using Raven.Client.Documents.Operations.ETL.Queue;
using Raven.Migrations;
using RavenDB.Samples.Library.Model;

namespace RavenDB.Samples.Library.Setup.Migrations;

[Migration(4)]
public sealed class ConfigureAzureQueueETL(MigrationContext context) : Migration
{
    private const string ConnectionStringName = "AzureQueueStorage";
    private const string QueueName = "timeouts";
    private const string EtlTaskName = "BorrowedBookTimeouts";

    public override void Up()
    {
        var connectionString = context.AzureStorageQueuesConnectionStringForRavenETL;

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Azure Storage Queue connection string not found. " +
                "Ensure the 'queues' connection string is configured via ConnectionStrings__queues environment variable.");
        }

        // 1. Register the Azure Queue connection string in RavenDB
        var queueConnectionString = new QueueConnectionString
        {
            Name = ConnectionStringName,
            BrokerType = QueueBrokerType.AzureQueueStorage,
            AzureQueueStorageConnectionSettings = new AzureQueueStorageConnectionSettings
            {
                ConnectionString = connectionString
            }
        };

        DocumentStore.Maintenance.Send(
            new PutConnectionStringOperation<QueueConnectionString>(queueConnectionString));

        // 2. Create the ETL configuration for BorrowedBook documents
        var etlConfiguration = new QueueEtlConfiguration
        {
            Name = EtlTaskName,
            ConnectionStringName = ConnectionStringName,
            BrokerType = QueueBrokerType.AzureQueueStorage,
            Queues =
            [
                new EtlQueue
                {
                    Name = QueueName
                }
            ],
            Transforms =
            [
                new Transformation
                {
                    Name = "BorrowedBookToTimeout",
                    Collections = { BorrowedBook.CollectionName },
                    Script = @"
                        var metadata = this['@metadata'];
                        if (metadata['@refresh'] == null && !this.ReturnedOn) {
                            // No refresh, for a borrowed book it means a potential timeout.
                            // Send the document ID to the queue
                            // Note: The ETL function name follows the pattern 'loadTo{QueueName}'
                            // where QueueName is the queue name with the first letter capitalized
                            loadToTimeouts({
                                id: id(this)
                            });
                        }
                    "
                }
            ]
        };

        // Add the ETL task to RavenDB
        DocumentStore.Maintenance.Send(
            new AddEtlOperation<QueueConnectionString>(etlConfiguration));
    }

    public override void Down()
    {
        // Note: Rolling back ETL configuration requires manual intervention through RavenDB Studio
        // or administrative operations. Automatic rollback is not implemented to avoid accidental
        // data loss or misconfiguration. The ETL task and connection string should be manually
        // disabled or removed if needed.
        throw new NotSupportedException(
            "Rolling back Azure Queue ETL configuration is not supported. " +
            "Please manually remove the ETL task and connection string through RavenDB Studio if needed.");
    }
}
