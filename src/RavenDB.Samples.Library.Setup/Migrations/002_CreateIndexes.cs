using Raven.Client.Documents.Indexes;
using Raven.Migrations;
using RavenDB.Samples.Library.Model.Indexes;

namespace RavenDB.Samples.Library.Setup.Migrations;

[Migration(2)]
public sealed class CreateIndexes : Migration
{
    private static readonly IAbstractIndexCreationTask[] Indexes =
    {
        new GlobalSearchIndex(),
        new BorrowedBooksByUserIdIndex(),
        new BooksByAuthor(),
    };
    
    public override void Up()
    {
        foreach (var index in Indexes)
        {
            index.Execute(DocumentStore);
        }
    }

    public override void Down()
    {
        foreach (var index in Indexes)
        {
            DocumentStore.Maintenance.Send(new Raven.Client.Documents.Operations.Indexes.DeleteIndexOperation(index.IndexName));
        }
    }
}
