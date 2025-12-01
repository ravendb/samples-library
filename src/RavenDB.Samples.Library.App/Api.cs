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
        var offset = GetQueryInt(req, "offset", 0, 0, 10000);
        var query = GetQueryString(req, "query");

        var booksQuery = string.IsNullOrWhiteSpace(query)
            ? session.Query<Book>()
            : session.Query<Book, Books_Search>().Search(b => b.Title, query);

        var books = await booksQuery
            .Skip(offset)
            .Take(10)
            .ToArrayAsync();

        return new JsonResult(books);
    }

    private static int GetQueryInt(HttpRequest req, string name, int defaultValue, int min, int max) =>
        req.Query.TryGetValue(name, out var value) && int.TryParse(value, out var parsed)
            ? Math.Clamp(parsed, min, max)
            : defaultValue;

    private static string? GetQueryString(HttpRequest req, string name) =>
        req.Query.TryGetValue(name, out var value) ? value.ToString() : null;

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