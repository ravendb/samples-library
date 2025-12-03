using Raven.Client.Documents.Indexes;

namespace RavenDB.Samples.Library.Model.Indexes;

public class BooksByAuthor : AbstractIndexCreationTask<Book>
{
    public BooksByAuthor()
    {
        Map = books => from book in books select new { book.AuthorId };
    }
}