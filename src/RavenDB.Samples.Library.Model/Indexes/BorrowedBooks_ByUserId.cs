using Raven.Client.Documents.Indexes;

namespace RavenDB.Samples.Library.Model.Indexes;

public class BorrowedBooks_ByUserId : AbstractIndexCreationTask<UserBook>
{
    public BorrowedBooks_ByUserId()
    {
        Map = userBooks => from userBook in userBooks
            where userBook.Returned == null
            select new
            {
                userBook.UserId,
                userBook.BookCopyId
            };
    }
}
