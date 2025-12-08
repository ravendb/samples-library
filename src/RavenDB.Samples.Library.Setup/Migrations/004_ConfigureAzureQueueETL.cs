using Raven.Client.Documents.Operations.ConnectionStrings;
using Raven.Client.Documents.Operations.ETL;
using Raven.Client.Documents.Operations.ETL.Queue;
using Raven.Migrations;

namespace RavenDB.Samples.Library.Setup.Migrations;

[Migration(4)]
public sealed class ConfigureAzureQueueETL : Migration
{
    private const string ConnectionStringName = "AzureQueueStorage";
    private const string QueueName = "timeouts";
    private const string EtlTaskName = "BorrowedBookTimeouts";

    public override void Up()
    {
        // Get the Azure Storage connection string from environment variables
        // Aspire injects this as ConnectionStrings__queues
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__queues");

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
                    Collections = { "BorrowedBooks" },
                    Script = @"
                        // Check if the borrowed book has no @refresh metadata
                        // Per requirements: 'if the borrowed book has no refresh, send a message to the Timeouts queue'
                        // 
                        // This condition will be true in these scenarios:
                        // 1. After RavenDB's refresh feature has processed the document at the scheduled time
                        //    and the @refresh metadata has been cleared
                        // 2. For any BorrowedBook documents that don't have a refresh scheduled
                        //
                        // The typical flow is:
                        // - Book borrowed → @refresh metadata set to BorrowedTo date
                        // - RavenDB refresh triggers at scheduled time → document updated
                        // - @refresh metadata cleared → ETL detects change and sends to queue
                        var metadata = this['@metadata'];
                        if (!metadata['@refresh']) {
                            // Send just the document ID to the queue
                            // Note: The ETL function name follows the pattern 'loadTo{QueueName}' (camelCase)
                            // where QueueName is the queue name with the first letter capitalized
                            loadToTimeouts({
                                Id: id(this)
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
