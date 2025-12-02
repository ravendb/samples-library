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
    
    [Function(nameof(Search))]
    public async Task<IActionResult> Search([HttpTrigger("get", Route = "search")] HttpRequest req)
    {
        var offset = req.GetQueryInt("offset", 0, 0, 10000);
        var query = req.GetQueryString("query");

        var queryable = session.Query<GlobalSearchIndex.Result, GlobalSearchIndex>();
        if (!string.IsNullOrEmpty(query))
        {
            queryable = queryable.Search(r => r.Query, query);
        }

        var results = await queryable
            .Skip(offset)
            .Take(10)
            .ToArrayAsync();

        return new JsonResult(results);
    }

    [Function(nameof(UsersGetById))]
    public async Task<IActionResult> UsersGetById([HttpTrigger("get", Route = "users/{id}")] HttpRequest req, string id)
    {
        var userId = $"Users/{id}";
        
        var user = await session.LoadAsync<User>(userId);
        
        if (user == null)
        {
            user = new User { Id = userId };
            await session.StoreAsync(user);
            await session.SaveChangesAsync();
        }
        
        var borrowedBooks = await session.Query<UserBook, BorrowedBooks_ByUserId>()
            .Include(x => x.BookId)
            .Where(x => x.UserId == userId)
            .ToArrayAsync();

        var bookIds = borrowedBooks.Select(x => x.BookId).ToArray();
        var books = await session.LoadAsync<Book>(bookIds);

        return new JsonResult(new
        {
            Id = user.Id,
            Borrowed = books.Values.ToArray()
        });
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