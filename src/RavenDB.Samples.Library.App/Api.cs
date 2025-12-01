using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.Migrations;
using RavenDB.Samples.Library.Model;
using RavenDB.Samples.Library.Model.Indexes;

namespace RavenDB.Samples.Library.App;

public class Api(ILogger<Api> logger, IAsyncDocumentSession session, IConfiguration config, MigrationRunner migrations)
{
    public const string EnvVarAdminCommandKeyName = "CommandKey";
    public const string HeaderAdminCommandKeyName = "X-Command-Key";

    [Function(nameof(BooksGetById))]
    public IActionResult BooksGetById([HttpTrigger("get", Route = "books/{id}")] HttpRequest req, string id)
    {
        return new JsonResult(new { Id = id, Name = "Test" });
    }
    
    [Function(nameof(BooksGet))]
    public async Task<IActionResult> BooksGet([HttpTrigger("get", Route = "books")] HttpRequest req)
    {
        var offset = 0;
        if (req.Query.TryGetValue("offset", out var offsetValue) && int.TryParse(offsetValue, out var parsedOffset))
        {
            offset = Math.Clamp(parsedOffset, 0, 10000);
        }

        var query = req.Query.TryGetValue("query", out var queryValue) ? queryValue.ToString() : null;

        IQueryable<Book> booksQuery;
        if (!string.IsNullOrWhiteSpace(query))
        {
            booksQuery = session.Query<Book, Books_Search>()
                .Search(b => b.Title, query);
        }
        else
        {
            booksQuery = session.Query<Book>();
        }

        var books = await booksQuery
            .Skip(offset)
            .Take(10)
            .ToArrayAsync();

        return new JsonResult(books);
    }

    [Function(nameof(Migrate))]
    public async Task<IActionResult> Migrate([HttpTrigger("post", Route = "migrate")] HttpRequest req)
    {
        var actual = req.Headers[HeaderAdminCommandKeyName];
        var expected = config.GetValue<string>(EnvVarAdminCommandKeyName);

        if (actual != expected)
        {
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }
        
        migrations.Run();
        
        return new StatusCodeResult(StatusCodes.Status202Accepted);
    }
}