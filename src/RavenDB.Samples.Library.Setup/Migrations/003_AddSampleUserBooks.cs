using Raven.Migrations;
using RavenDB.Samples.Library.Model;

namespace RavenDB.Samples.Library.Setup.Migrations;

[Migration(3)]
public sealed class AddSampleUserBooks : Migration
{
    public override void Up()
    {
        // This migration adds sample borrowed books for testing purposes
        // Get a sample user and books to create borrowed book records
        
        using var session = DocumentStore.OpenSession();
        
        // Get first book and copy for testing
        var book = session.Query<Book>().FirstOrDefault();
        if (book == null)
            return; // No books available
            
        var bookCopy = session.Query<BookCopy>()
            .Where(bc => bc.BookId == book.Id && bc.Status == BookCopyStatus.Available)
            .FirstOrDefault();
            
        if (bookCopy == null)
            return; // No available copies
        
        // Create a test user if needed
        var testUserId = User.BuildId("test-user");
        var user = session.Load<User>(testUserId);
        if (user == null)
        {
            user = new User { Id = testUserId };
            session.Store(user);
        }
        
        // Create an overdue book (due 7 days ago)
        var overdueUserBook = new UserBook
        {
            Id = "UserBooks/test-overdue-1",
            UserId = testUserId,
            BookId = book.Id,
            BookCopyId = bookCopy.Id,
            Borrowed = DateTime.UtcNow.AddDays(-21),
            DueDate = DateTime.UtcNow.AddDays(-7),
            Returned = null
        };
        session.Store(overdueUserBook);
        
        // Mark the book copy as borrowed
        bookCopy.Status = BookCopyStatus.Borrowed;
        
        // Create an active book (due in 7 days)
        var secondBook = session.Query<Book>().Skip(1).FirstOrDefault();
        if (secondBook != null)
        {
            var secondBookCopy = session.Query<BookCopy>()
                .Where(bc => bc.BookId == secondBook.Id && bc.Status == BookCopyStatus.Available)
                .FirstOrDefault();
                
            if (secondBookCopy != null)
            {
                var activeUserBook = new UserBook
                {
                    Id = "UserBooks/test-active-1",
                    UserId = testUserId,
                    BookId = secondBook.Id,
                    BookCopyId = secondBookCopy.Id,
                    Borrowed = DateTime.UtcNow.AddDays(-7),
                    DueDate = DateTime.UtcNow.AddDays(7),
                    Returned = null
                };
                session.Store(activeUserBook);
                
                // Mark the book copy as borrowed
                secondBookCopy.Status = BookCopyStatus.Borrowed;
            }
        }
        
        session.SaveChanges();
    }
    
    public override void Down()
    {
        // Clean up sample data
        using var session = DocumentStore.OpenSession();
        
        var userBooksToDelete = session.Query<UserBook>()
            .Where(ub => ub.Id.StartsWith("UserBooks/test-"))
            .ToList();
            
        foreach (var userBook in userBooksToDelete)
        {
            // Reset book copy status
            var bookCopy = session.Load<BookCopy>(userBook.BookCopyId);
            if (bookCopy != null)
            {
                bookCopy.Status = BookCopyStatus.Available;
            }
            
            session.Delete(userBook);
        }
        
        session.SaveChanges();
    }
}
