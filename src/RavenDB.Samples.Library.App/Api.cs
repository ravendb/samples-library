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
        var offset = req.GetQueryInt("offset", 0, 0, 10000);
        var query = req.GetQueryString("query");

        var booksQuery = string.IsNullOrWhiteSpace(query)
            ? session.Query<Book>()
            : session.Query<Global_Search.Result, Global_Search>()
                .Search(r => r.Query, query)
                .OfType<Book>();

        var books = await booksQuery
            .Skip(offset)
            .Take(10)
            .ToArrayAsync();

        return new JsonResult(books);
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
            .Include(x => x.BookCopyId)
            .Where(x => x.UserId == userId)
            .ToArrayAsync();

        var bookCopyIds = borrowedBooks.Select(x => x.BookCopyId).ToArray();
        var bookCopies = await session.LoadAsync<BookCopy>(bookCopyIds);

        return new JsonResult(new
        {
            id = user.Id,
            borrowed = bookCopies.Values.ToArray()
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