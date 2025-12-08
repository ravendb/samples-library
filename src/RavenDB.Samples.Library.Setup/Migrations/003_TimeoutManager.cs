using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Operations.Refresh;
using Raven.Migrations;
using RavenDB.Samples.Library.Model.Indexes;

namespace RavenDB.Samples.Library.Setup.Migrations;

[Migration(3)]
public sealed class TimeoutManagerMigration : Migration
{
    public override void Up()
    {
        SetupRefresh(true);
    }

    public override void Down()
    {
        SetupRefresh(false);
    }

    private void SetupRefresh(bool enabled)
    {
        var refreshConfig = new RefreshConfiguration
        {
            Disabled = !enabled,
            RefreshFrequencyInSec = 15,
            MaxItemsToProcess = 1024
        };
        DocumentStore.Maintenance.Send(new ConfigureRefreshOperation(refreshConfig));
    }
}
