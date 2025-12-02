using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using RavenDB.Samples.Library.Model;
using RavenDB.Samples.Library.Model.Indexes;

namespace RavenDB.Samples.Library.App;

public class UserApi(IAsyncDocumentSession session)
{
    public const string HeaderUserIdName = "X-User-Id";

    [Function(nameof(UserGet))]
    public async Task<IActionResult> UserGet([HttpTrigger("get", Route = "user/profile")] HttpRequest req)
    {
        if (!TryGetUserId(req, out var userId))
        {
            return new UnauthorizedResult();
        }
        
        var user = await session.LoadAsync<User>(userId);
        
        if (user == null)
        {
            user = new User { Id = userId };
            await session.StoreAsync(user);
            await session.SaveChangesAsync();

            // A user has no books, if it was just created.
            return GetProfile(user, []);
        }
        
        var borrowedBooks = await session.Query<UserBook, BorrowedBooksByUserIdIndex>()
            .Include(x => x.BookId)
            .Where(x => x.UserId == userId)
            .ToArrayAsync();

        var bookIds = borrowedBooks.Select(x => x.BookId).ToArray();
        var books = await session.LoadAsync<Book>(bookIds);

        return GetProfile(user, books.Values);
    }

    private static JsonResult GetProfile(User user, IEnumerable<Book> books)
    {
        return new JsonResult(new
        {
            Id = user.Id,
            Borrowed = books.ToArray()
        });
    }

    private static bool TryGetUserId(HttpRequest req, [MaybeNullWhen(false)] out string userId)
    {
        if (req.Headers.TryGetValue(HeaderUserIdName, out var headerValue) && 
            !string.IsNullOrWhiteSpace(headerValue))
        {
            var value = headerValue.ToString().Trim();
            
            // Validate that the header value contains only safe characters
            if (value.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'))
            {
                userId = User.BuildId(value);
                return true;
            }
        }

        userId = null;
        return false;
    }
}
