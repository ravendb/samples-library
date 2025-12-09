namespace RavenDB.Samples.Library.Model;

/// <summary>
/// Represents a book borrowed by a user.
/// </summary>
public class BorrowedBook : IRoot
{
    public static readonly TimeSpan BorrowFor =  TimeSpan.FromSeconds(30);
    
    public string Id { get; set; }

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

    public DateTimeOffset? ReturnedOn { get; set; }

    private const string IdPrefix = "BorrowedBooks";
    
    public static string BuildId(string value) => $"{IdPrefix}/{value}";

    public static bool IsIdOf(string id) => id.StartsWith(IdPrefix);
}
