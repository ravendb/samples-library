using Raven.Client.Documents.Indexes;

namespace RavenDB.Samples.Library.Model.Indexes;

public class BookCopyAvailabilityIndex : AbstractIndexCreationTask<BookCopy, BookCopyAvailabilityIndex.Result>
{
    public class Result
    {
        public string BookId  { get; set; }
        public int Available { get; set; }
        public int Total { get; set; }
    }
    
    public BookCopyAvailabilityIndex()
    {
        Map = copies => from copy in copies
            select new Result
            {
                BookId = copy.BookId,
                Available = copy.Status == BookCopyStatus.Available ? 1 : 0,
                Total = 1,
            };

        Reduce = results =>
            from result in results
            group result by result.BookId
            into g
            select new Result
            {
                BookId = g.Key,
                Available = g.Sum(x => x.Available),
                Total = g.Sum(x => x.Total)
            };
    }
}