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
            return new JsonResult(new
            {
                user.Id,
                Borrowed = Array.Empty<object>()
            });
        }
        
        var borrowedBooks = await session.Query<BorrowedBook>()
            .Include(x => x.BookId)
            .Where(x => x.UserId == userId && x.ReturnedOn == null)
            .ToArrayAsync();

        var borrowed = new List<object>(borrowedBooks.Length);

        foreach (var borrowedBook in borrowedBooks)
        {
            var book = await session.LoadAsync<Book>(borrowedBook.BookId);

            borrowed.Add(
                new
                {
                    Id = borrowedBook.Id,
                    Overdue = borrowedBook.BorrowedTo < DateTimeOffset.Now,
                    BookId = book.Id,
                    Title = book.Title,
                });
        }

        return new JsonResult(new
        {
            user.Id,
            Borrowed = borrowed
        });
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
    
    [Function(nameof(NotificationsCount))]
    public async Task<IActionResult> NotificationsCount([HttpTrigger("get", Route = "user/notifications/count")] HttpRequest req)
    {
        if (!TryGetUserId(req, out var userId))
        {
            return new UnauthorizedResult();
        }
        
        // Notifications count
        var count = await session.Query<Notification>()
            .Where(x => x.UserId == userId)
            .CountAsync();

        return new JsonResult(new { Count = count });
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

    [Function(nameof(UserBookReturn))]
    public async Task<IActionResult> UserBookReturn([HttpTrigger("post", Route = "user/books/{id}/return")] HttpRequest req, string id)
    {
        if (!TryGetUserId(req, out var userId))
        {
            return new UnauthorizedResult();
        }

        var userBookId = BorrowedBook.BuildId(id);
        var borrowed = await session.LoadAsync<BorrowedBook>(userBookId);

        if (borrowed == null)
        {
            return new NotFoundResult();
        }

        // Verify the book belongs to the user
        if (borrowed.UserId != userId)
        {
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        // Check if already returned
        if (borrowed.ReturnedOn != null)
        {
            return new StatusCodeResult(StatusCodes.Status400BadRequest);
        }

        // Concurrency checks
        session.Advanced.UseOptimisticConcurrency = true;
        
        // Mark as returned
        borrowed.ReturnedOn = DateTimeOffset.UtcNow;

        // Update book copy status to available
        var bookCopy = await session.LoadAsync<BookCopy>(borrowed.BookCopyId);
        bookCopy.Status = BookCopyStatus.Available;

        await session.SaveChangesAsync();

        return new StatusCodeResult(StatusCodes.Status204NoContent);
    }

    [Function(nameof(UserBookBorrowPost))]
    public async Task<IActionResult> UserBookBorrowPost([HttpTrigger("post", Route = "user/books")] HttpRequest req)
    {
        if (!TryGetUserId(req, out var userId))
        {
            return new UnauthorizedResult();
        }

        var payload = await req.ReadFromJsonAsync<BorrowPayload>();
        var id = Book.BuildId(payload.BookId);
        
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
        
        // Mark the copy a borrowed.
        copy.Status = BookCopyStatus.Borrowed;

        // Capture the state of the borrowed book as a document
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
        
        return new CreatedResult($"user/books/{borrowed.Id}", borrowed);
    }
    
    private class BorrowPayload 
    {
        public string BookId { get; set; }
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
