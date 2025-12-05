namespace RavenDB.Samples.Library.Model;

/// <summary>
/// Represents a book borrowed by a user.
/// </summary>
public class BorrowedBook : IRoot
{
    public static readonly TimeSpan BorrowFor =  TimeSpan.FromSeconds(30);
    
    public string Id { get; set; }
    
    public static string BuildId(string value) => $"BorrowedBooks/{value}";

    /// <summary>
    /// The user who borrowed the book.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// The book copy that was borrowed.
    /// </summary>
    public string BookCopyId { get; set; }

    /// <summary>
    /// The book that was borrowed.
    /// </summary>
    public string BookId { get; set; }

    /// <summary>
    /// The date when the book was borrowed.
    /// </summary>
    public DateTimeOffset BorrowedFrom { get; set; }

    /// <summary>
    /// The date, when the book copy should be returned.
    /// </summary>
    public DateTimeOffset BorrowedTo { get; set; }
    
}
