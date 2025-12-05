using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using RavenDB.Samples.Library.Model;

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
        
        var (created, user) = await TryCreateUser(userId);

        if (created)
        {
            // A user has no books, if it was just created.
            return GetProfile(user, []);   
        }
        
        var borrowedBooks = await session.Query<BorrowedBook>()
            .Include(x => x.BookId)
            .Where(x => x.UserId == userId)
            .ToArrayAsync();

        var bookIds = borrowedBooks.Select(x => x.BookId).ToArray();
        var books = await session.LoadAsync<Book>(bookIds);

        return GetProfile(user, books.Values);
    }

    [Function(nameof(NotificationsGet))]
    public async Task<IActionResult> NotificationsGet([HttpTrigger("get", Route = "user/notifications")] HttpRequest req)
    {
        if (!TryGetUserId(req, out var userId))
        {
            return new UnauthorizedResult();
        }
        
        var (created, _) = await TryCreateUser(userId);

        if (created)
        {
            // A created user has no notifications
            return new JsonResult(Array.Empty<object>());
        }

        // Max notifications
        const int max = 25;
        var notifications = await session.Query<Notification>()
            .Where(x => x.UserId == userId)
            .Take(max)
            .ToArrayAsync();

        return new JsonResult(notifications.Select(notification => new { notification.Id, notification.Text, notification.ReferencedItemId }));
    }

    [Function(nameof(NotificationDelete))]
    public async Task<IActionResult> NotificationDelete([HttpTrigger("delete", Route = "user/notifications/{id}")] HttpRequest req, string id)
    {
        if (!TryGetUserId(req, out var userId))
        {
            return new UnauthorizedResult();
        }

        var notificationId = Notification.BuildId(id);
        var notification = await session.LoadAsync<Notification>(notificationId);

        if (notification == null)
        {
            return new NotFoundResult();
        }

        // Verify the notification belongs to the user
        if (notification.UserId != userId)
        {
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        session.Delete(notification);
        await session.SaveChangesAsync();

        return new StatusCodeResult(StatusCodes.Status204NoContent);
    }

    [Function(nameof(UserBookBorrowPost))]
    private async Task<IActionResult> UserBookBorrowPost([HttpTrigger("post", Route = "user/books/borrow/{id}")] HttpRequest req, string id)
    {
        if (!TryGetUserId(req, out var userId))
        {
            return new UnauthorizedResult();
        }

        // For high concurrent usage rate, this wouldn't work. We'd need to have and we'd need to have a way of better scaling it.
        // At the same time, we might use retrying.
        // If that was a physical book delivered to a desk, a scan of the book copy id would be sufficent. Nobody can hold the same book copy at the same time. 
        var availableCopies = await session.Query<BookCopy>()
            .Where(copy => copy.BookId == id && copy.Status == BookCopyStatus.Available)
            .Take(1)
            .ToArrayAsync();

        if (availableCopies.Length == 0)
            return new NotFoundResult();

        var copy = availableCopies.Single();
        
        session.Advanced.UseOptimisticConcurrency = true;
        
        copy.Status = BookCopyStatus.Borrowed;

        var borrowed = new BorrowedBook
        {
            BookCopyId = copy.Id, BookId = copy.BookId, UserId = userId,
            BorrowedFrom = DateTimeOffset.Now,
            BorrowedTo = DateTimeOffset.Now + BorrowedBook.BorrowFor
        };
        
        await session.StoreAsync(borrowed);

        try
        {
            await session.SaveChangesAsync();
        }
        catch
        {
            // Can't save due to concurrency issue. Return conflict
            return new ConflictResult();
        }
        
        return new CreatedResult($"user/borrowedbooks/{borrowed.Id}", borrowed);
    }
    
    private static JsonResult GetProfile(User user, IEnumerable<Book> books)
    {
        return new JsonResult(new
        {
            Id = user.Id,
            Borrowed = books.ToArray()
        });
    }

    private async Task<(bool created, User user)> TryCreateUser(string userId)
    {
        var lazyBook = session.Query<Book>().Customize(customize => customize.RandomOrdering()).Take(1).LazilyAsync();
        var user = await session.LoadAsync<User>(userId);

        if (user != null) 
            return (false, user);
        
        user = new User { Id = userId };
        await session.StoreAsync(user);
        var book = (await lazyBook.Value).Single();
        await session.StoreAsync(new Notification { UserId = userId, Text = "Welcome in the Library of Ravens! 💙 Check out this random book!", Id = Notification.GetNewId(), ReferencedItemId = book.Id});
        await session.SaveChangesAsync();
            
        return (true, user);
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
