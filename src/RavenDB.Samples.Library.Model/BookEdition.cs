namespace RavenDB.Samples.Library.Model;

/// <summary>
/// An edition of a <see cref="Book"/>.
/// </summary>
public class BookEdition
{
    public string Id { get; set; }

    public string BookId { get; set; }

    public string Title { get; set; }

    public long GoodreadsBookId { get; set; }

    public long BestBookId { get; set; }

    public int BooksCount { get; set; }

    public string? Isbn { get; set; }

    public string? Isbn13 { get; set; }

    public string? LanguageCode { get; set; }

    public string? ImageUrl { get; set; }

    public string? SmallImageUrl { get; set; }
}
