using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace RavenDB.Samples.Library.App;

public class QueueApi(ILogger<QueueApi> logger)
{
    [Function(nameof(ProcessTimeout))]
    public void ProcessTimeout(
        [QueueTrigger("timeouts", Connection = "queues")] string message)
    {
        // Log the received message
        logger.LogInformation("Received timeout message: {Message}", message);
        
        // TODO: Implement timeout processing logic
        // For now, just consume the message
    }
}
