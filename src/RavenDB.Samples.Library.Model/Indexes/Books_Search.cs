using Raven.Client.Documents.Indexes;

namespace RavenDB.Samples.Library.Model.Indexes;

public class Books_Search : AbstractIndexCreationTask<Book>
{
    public Books_Search()
    {
        Map = books => from book in books
            select new
            {
                book.Title
            };

        Indexes.Add(x => x.Title, FieldIndexing.Search);
    }
}
