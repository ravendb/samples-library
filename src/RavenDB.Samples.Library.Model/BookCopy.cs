namespace RavenDB.Samples.Library.Model;

/// <summary>
/// A copy of a <see cref="Book"/> that is of a given <see cref="BookEdition"/>.
/// </summary>
public class BookCopy
{
    public string Id { get; set; }

    /// <summary>
    /// <see cref="BookEdition"/> that this copy is of.
    /// </summary>
    public string BookEditionId { get; set; }
    
    public string BookId { get; set; }
    
    public BookCopyStatus  Status { get; set; }
}

public enum BookCopyStatus
{
    Available,
    Borrowed
}