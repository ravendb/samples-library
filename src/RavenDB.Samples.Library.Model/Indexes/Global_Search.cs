using Raven.Client.Documents.Indexes;

namespace RavenDB.Samples.Library.Model.Indexes;

public class Global_Search : AbstractMultiMapIndexCreationTask<Global_Search.Result>
{
    public class Result
    {
        public string Id { get; set; }
        public string Query { get; set; }
    }

    public Global_Search()
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
    }
}
