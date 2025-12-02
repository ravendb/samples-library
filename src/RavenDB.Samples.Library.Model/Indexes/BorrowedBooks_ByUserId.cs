using Raven.Client.Documents.Indexes;

namespace RavenDB.Samples.Library.Model.Indexes;

public class BorrowedBooksByUserId : AbstractIndexCreationTask<UserBook>
{
    public BorrowedBooksByUserId()
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
