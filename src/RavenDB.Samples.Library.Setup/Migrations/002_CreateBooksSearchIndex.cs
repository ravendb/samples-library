using Raven.Client.Documents.Indexes;
using Raven.Migrations;
using RavenDB.Samples.Library.Model;

namespace RavenDB.Samples.Library.Setup.Migrations;

[Migration(2)]
public sealed class CreateBooksSearchIndex : Migration
{
    public override void Up()
    {
        new Books_Search().Execute(DocumentStore);
    }

    public override void Down()
    {
        DocumentStore.Maintenance.Send(
            new Raven.Client.Documents.Operations.Indexes.DeleteIndexOperation(new Books_Search().IndexName));
    }
}

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
