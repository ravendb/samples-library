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
    public async Task<IActionResult> BooksGetById([HttpTrigger("get", Route = "books/{id}")] HttpRequest req, string id)
    {
        // Fetch Author using the Include method. This will result in a single round trip to server
        var book = await session
            .Include<Book>(b => b.AuthorId)
            .LoadAsync<Book>(Book.BuildId(id));

        if (book == null)
            return new NotFoundResult();

        var author = await session.LoadAsync<Author>(book.AuthorId);

        return new JsonResult(
            new
            {
                book.Id, book.Title, Author = author
            });
    }

    [Function(nameof(AuthorsGetById))]
    public async Task<IActionResult> AuthorsGetById([HttpTrigger("get", Route = "authors/{id}")] HttpRequest req, string id)
    {
        var authorId = Author.BuildId(id);
        
        // Lazily fetch author's books. We use LazilyAsync to make it happen in one request to the database.
        var lazyBooks = session.Query<Book, BooksByAuthor>()
            .Where(book => book.AuthorId == authorId)
            .LazilyAsync();

        var author = await session.LoadAsync<Author>(authorId);
        var books = await lazyBooks.Value;
        
        if (author == null)
            return new NotFoundResult();
        
        return new JsonResult(new 
        {
            author.Id, author.FirstName, author.LastName,
            Books = books.Select(static book => new {book.Id, book.Title})
        });
    }

    [Function(nameof(Search))]
    public async Task<IActionResult> Search([HttpTrigger("get", Route = "search")] HttpRequest req)
    {
        var offset = req.GetQueryInt("offset", 0, 0, 10000);
        var query = req.GetQueryString("query");

        var queryable = session
            .Query<GlobalSearchIndex.Result, GlobalSearchIndex>()
            .Include(r => r.Id);

        if (!string.IsNullOrEmpty(query))
        {
            queryable = queryable.Search(r => r.Query, query);
        }

        var results = await queryable
            .Statistics(out var stats) // Extract statistics to use it for caching
            .Skip(offset)
            .Take(10)
            .ToArrayAsync();

        //return new JsonResult(results);
        return req.TryCachePublicly(new JsonResult(results), stats);
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