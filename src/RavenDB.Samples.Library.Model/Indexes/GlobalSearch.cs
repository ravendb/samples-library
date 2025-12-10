using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Indexes.Vector;

namespace RavenDB.Samples.Library.Model.Indexes;

public static class GlobalSearch
{
    public class Result
    {
        public string Id { get; set; }
        public string Query { get; set; }
    }

    public class FullTextIndex : AbstractMultiMapIndexCreationTask<Result>
    {
        public FullTextIndex()
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

    public class VectorIndex : AbstractMultiMapIndexCreationTask<VectorIndex.VectorResult>
    {
        public class VectorResult : Result
        {
            public object VectorFromDescription { get; set; }
        }

        public VectorIndex()
        {
            AddMap<Book>(books => from book in books
                select new VectorResult
                {
                    Id = book.Id,
                    Query = book.Title,
                    VectorFromDescription = CreateVector(book.Description)
                });

            AddMap<Author>(authors => from author in authors
                select new VectorResult
                {
                    Id = author.Id,
                    Query = author.FirstName + " " + author.LastName,
                    // The author has no description, let's use their names
                    VectorFromDescription = CreateVector(author.FirstName + " " + author.LastName)
                });

            VectorIndexes.Add(x => x.VectorFromDescription, new VectorOptions(
            )
            {
                SourceEmbeddingType = VectorEmbeddingType.Text,
                DestinationEmbeddingType = VectorEmbeddingType.Single
            });

            SearchEngineType = Raven.Client.Documents.Indexes.SearchEngineType.Corax;

            Store(x => x.Query, FieldStorage.Yes);
        }
    }
}