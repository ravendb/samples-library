using Raven.Migrations;
using RavenDB.Samples.Library.Model.Indexes;

namespace RavenDB.Samples.Library.Setup.Migrations;

[Migration(3)]
public sealed class CreateBorrowedBooksIndex : Migration
{
    public override void Up()
    {
        new BorrowedBooksByUserId().Execute(DocumentStore);
    }

    public override void Down()
    {
        DocumentStore.Maintenance.Send(
            new Raven.Client.Documents.Operations.Indexes.DeleteIndexOperation(new BorrowedBooksByUserId().IndexName));
    }
}
