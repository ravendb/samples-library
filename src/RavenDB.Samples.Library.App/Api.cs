using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace RavenDB.Samples.Library.App;

public class Api
{
    private readonly ILogger<Api> _logger;

    public Api(ILogger<Api> logger)
    {
        _logger = logger;
    }

    [Function(nameof(BooksGetById))]
    public IActionResult BooksGetById([HttpTrigger("get", Route = "books/{id}")] HttpRequest req, string id)
    {
        return new JsonResult(new { Id = id, Name = "Test" });
    }
    
    [Function(nameof(BooksGet))]
    public IActionResult BooksGet([HttpTrigger("get", Route = "books")] HttpRequest req)
    {
        return new JsonResult(new[]
        {
            new { Id = 1, Name = "Test" },
            new { Id = 2, Name = "Test" }
        });
    }
}