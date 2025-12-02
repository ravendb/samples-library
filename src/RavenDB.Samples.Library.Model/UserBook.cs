namespace RavenDB.Samples.Library.Model;

/// <summary>
/// Represents a book borrowed by a user.
/// </summary>
public class UserBook
{
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
    public DateTime Borrowed { get; set; }

    /// <summary>
    /// The date when the book was returned. Null if not yet returned.
    /// </summary>
    public DateTime? Returned { get; set; }
}
