using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents.Session;
using RavenDB.Samples.Library.Model;

namespace RavenDB.Samples.Library.App;

public class Timeouts(IAsyncDocumentSession session, ILogger<Timeouts> logger)
{
    [Function(nameof(ProcessTimeout))]
    public void ProcessTimeout([QueueTrigger("timeouts", Connection = "BindingConnection")] CloudEventMessage? msg)
    {
        if (msg?.Data?.Id == null)
        {
            throw new Exception("The id of the underlying document could't be parsed");
        }

        var id = msg.Data.Id;
        
        if (BorrowedBook.IsIdOf(id))
        {
            logger.LogInformation("Received timeout message: {Message}", id);
        }
    }
}

/// <summary>
/// We could use the library <see cref="https://github.com/cloudevents/sdk-csharp"/> but as it's only for the identifier,
/// we're good with a simple schema.
/// </summary>
public class CloudEventMessage
{
    public TimeoutData Data { get; set; }

    public class TimeoutData
    {
        public string Id { get; set; }
    }
}