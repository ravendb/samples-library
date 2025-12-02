using Raven.Client.Documents.Indexes;
using Raven.Migrations;
using RavenDB.Samples.Library.Model.Indexes;

namespace RavenDB.Samples.Library.Setup.Migrations;

[Migration(2)]
public sealed class CreateIndexes : Migration
{
    public override void Up()
    {
        new Global_Search().Execute(DocumentStore);
    }

    public override void Down()
    {
        DocumentStore.Maintenance.Send(
            new Raven.Client.Documents.Operations.Indexes.DeleteIndexOperation(new Global_Search().IndexName));
    }
}
