namespace RavenDB.Samples.Library.Model;

/// <summary>
/// An edition of a <see cref="Book"/>.
/// </summary>
public class BookEdition
{
    public string Id { get; set; }

    public string BookId { get; set; }

    public string Title { get; set; }

    public string? Isbn { get; set; }

    public string? LanguageCode { get; set; }

    public string? ImageUrl { get; set; }

    public string? SmallImageUrl { get; set; }
}
