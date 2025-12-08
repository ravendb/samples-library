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
                        // Only process borrowed books that have no refresh metadata
                        // (meaning they haven't been scheduled for timeout yet)
                        var metadata = this['@metadata'];
                        if (!metadata['@refresh']) {
                            // Send just the document ID to the queue
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
        // ETL and connection string removal would typically be done manually through RavenDB Studio
        // or with specific administrative operations. For migrations, we'll leave this as a no-op.
        // The ETL can be disabled or removed through the RavenDB Studio if needed.
    }
}
