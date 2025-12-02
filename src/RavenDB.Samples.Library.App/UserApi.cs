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
    public async Task<IActionResult> UserGet([HttpTrigger("get", Route = "users")] HttpRequest req)
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

    private static bool TryGetUserId(HttpRequest req, out string userId)
    {
        if (req.Headers.TryGetValue(HeaderUserIdName, out var headerValue) && 
            !string.IsNullOrWhiteSpace(headerValue))
        {
            var value = headerValue.ToString().Trim();
            
            // Validate that the header value contains only safe characters
            if (value.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'))
            {
                userId = $"Users/{value}";
                return true;
            }
        }

        userId = string.Empty;
        return false;
    }
}
