using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RavenDB.Samples.Library.Model;

namespace RavenDB.Samples.Library.App;

public class Timeouts(ILogger<Timeouts> logger)
{
    [Function(nameof(ProcessTimeout))]
    public void ProcessTimeout([QueueTrigger("timeouts", Connection = "BindingConnection")] string id)
    {
        if (BorrowedBook.IsIdOf(id))
        {
            logger.LogInformation("Received timeout message: {Message}", id);    
        }// Log the received message
    }
}
