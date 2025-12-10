using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
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
        var bookId = Book.BuildId(id);

        // Use a query against the map-reduce index
        var lazyAvailability = Queryable.Where(session.Query<BookCopyAvailabilityIndex.Result, BookCopyAvailabilityIndex>()
                .Statistics(out var stats), availability => availability.BookId == bookId)
            .LazilyAsync();

        // Fetch Author using the Include method. This will result in a single round trip to server
        var book = await session
            .Include<Book>(b => b.AuthorId)
            .LoadAsync<Book>(bookId);

        if (book == null)
            return new NotFoundResult();

        var author = await session.LoadAsync<Author>(book.AuthorId);

        var availability = (await lazyAvailability.Value).Single();

        var result = new JsonResult(
            new
            {
                book.Id, book.Title, book.Description,
                Author = author,
                Availability = new { availability.Available, availability.Total }
            });

        // Combine all inputs for the stats
        return req.TryCachePublicly(result, stats, session, author, book);
    }

    [Function(nameof(AuthorsGetById))]
    public async Task<IActionResult> AuthorsGetById([HttpTrigger("get", Route = "authors/{id}")] HttpRequest req, string id)
    {
        var authorId = Author.BuildId(id);

        // Lazily fetch author's books. We use LazilyAsync to make it happen in one request to the database.
        var lazyBooks = Queryable.Where(session.Query<Book, BooksByAuthor>()
                .Statistics(out var stats), book => book.AuthorId == authorId)
            .LazilyAsync();

        var author = await session.LoadAsync<Author>(authorId);
        var books = await lazyBooks.Value;

        if (author == null)
            return new NotFoundResult();

        var result = new JsonResult(new
        {
            author.Id, author.FirstName, author.LastName,
            Books = books.Select(static book => new { book.Id, book.Title })
        });

        return req.TryCachePublicly(result, stats, session, author);
    }

    [Function(nameof(Search))]
    public async Task<IActionResult> Search([HttpTrigger("get", Route = "search")] HttpRequest req)
    {
        var offset = req.GetQueryInt("offset", 0, 0, 10000);
        var query = req.GetQueryString("query");
        var isVector = req.GetQueryString("vector") != null;

        var nonEmptyQuery = !string.IsNullOrEmpty(query);

        (QueryStatistics stats, GlobalSearch.Result[] results) result;
        if (isVector && nonEmptyQuery)
        {
            var queryable = session.Query<GlobalSearch.VectorIndex.VectorResult, GlobalSearch.VectorIndex>();
            queryable = queryable.VectorSearch(
                field => field.WithField(x => x.VectorFromDescription),
                embedding => embedding.ByText(query));

            result = await GetSearchResult(queryable, offset);
        }
        else
        {
            var queryable = session.Query<GlobalSearch.Result, GlobalSearch.FullTextIndex>();
            if (nonEmptyQuery)
            {
                queryable = queryable.Search(r => r.Query, query);
            }

            result = await GetSearchResult(queryable, offset);
        }

        return req.TryCachePublicly(new JsonResult(result.results), result.stats);

        static async Task<(QueryStatistics stats, GlobalSearch.Result[] results)> GetSearchResult<T>(IRavenQueryable<T> ravenQueryable, int skip)
            where T : GlobalSearch.Result
        {
            var toArrayAsync = await ravenQueryable
                .Statistics(out var queryStatistics) // Extract statistics to use it for caching
                .ProjectInto<GlobalSearch.Result>() // We need to inform Raven, what fields we want to project to. This does it for all the type properties
                .Skip(skip)
                .Take(10)
                .ToArrayAsync();

            return (queryStatistics, toArrayAsync);
        }
    }

    [Function(nameof(HomeBooks))]
    public async Task<IActionResult> HomeBooks([HttpTrigger("get", Route = "home/books")] HttpRequest req)
    {
        const int count = 8;

        var books = await Queryable.Take(session.Query<Book>()
                .Include(b => b.AuthorId) // Include the author so that further loads of authors do not issue requests to the server.
                .Customize(customize => customize.RandomOrdering()), count)
            .ToArrayAsync();

        var results = new List<object>(count);
        foreach (var book in books)
        {
            // These loads will not issue requests to the server, they are already prefetched with Include above.
            var author = await session.LoadAsync<Author>(book.AuthorId);
            results.Add(new { book.Id, book.Title, Author = author });
        }

        // Don't cache the home books. They are randomized.
        return new JsonResult(results);
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