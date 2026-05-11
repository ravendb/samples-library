using Raven.Migrations;

namespace RavenDB.Samples.Library.Seeder;

internal sealed class SampleDataSeeder(MigrationRunner runner)
{
    public Task SeedAllAsync(CancellationToken ct) => Task.Run(runner.Run, ct);
}
