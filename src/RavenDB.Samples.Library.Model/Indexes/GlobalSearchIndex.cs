using Raven.Client.Documents.Indexes;

namespace RavenDB.Samples.Library.Model.Indexes;

public class GlobalSearchIndex : AbstractMultiMapIndexCreationTask<GlobalSearchIndex.Result>
{
    public class Result
    {
        public string Id { get; set; }
        public string Query { get; set; }
    }

    public GlobalSearchIndex()
    {
        AddMap<Book>(books => from book in books
            select new Result
            {
                Id = book.Id,
                Query = book.Title
            });

        AddMap<Author>(authors => from author in authors
            select new Result
            {
                Id = author.Id,
                Query = author.FirstName + " " + author.LastName
            });

        Index(x => x.Query, FieldIndexing.Search);
        Store(x => x.Query, FieldStorage.Yes);
    }
}
