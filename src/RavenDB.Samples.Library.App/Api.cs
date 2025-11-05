using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using RavenDB.Samples.Library.Model;

namespace RavenDB.Samples.Library.App;

public class Api(ILogger<Api> logger, IAsyncDocumentSession session)
{
    [Function(nameof(BooksGetById))]
    public IActionResult BooksGetById([HttpTrigger("get", Route = "books/{id}")] HttpRequest req, string id)
    {
        return new JsonResult(new { Id = id, Name = "Test" });
    }
    
    [Function(nameof(BooksGet))]
    public async Task<IActionResult> BooksGet([HttpTrigger("get", Route = "books")] HttpRequest req)
    {
        var books = await session.Query<Book>()
            .Take(10)
            .ToArrayAsync();

        return new JsonResult(books);
    }
}