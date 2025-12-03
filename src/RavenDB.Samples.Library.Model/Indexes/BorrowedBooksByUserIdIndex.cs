using Raven.Client.Documents.Indexes;

namespace RavenDB.Samples.Library.Model.Indexes;

public class BorrowedBooksByUserIdIndex : AbstractIndexCreationTask<UserBook>
{
    public BorrowedBooksByUserIdIndex()
    {
        Map = userBooks => from userBook in userBooks
            where userBook.Returned == null
            select new
            {
                userBook.UserId,
                userBook.BookCopyId,
                userBook.BookId
            };
    }
}