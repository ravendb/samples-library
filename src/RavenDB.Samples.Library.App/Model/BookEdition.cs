namespace RavenDB.Samples.Library.App.Model;

/// <summary>
/// An edition of a <see cref="Book"/>.
/// </summary>
public class BookEdition
{
    public string Id { get; set; }

    public string BookId { get; set; }
}