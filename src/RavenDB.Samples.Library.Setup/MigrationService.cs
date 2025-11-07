using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raven.Migrations;

namespace RavenDB.Samples.Library.Setup;

public class MigrationService(MigrationRunner runner, ILogger<MigrationService> service) : IHostedService
{
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		// Run all the migrations
		runner.Run();
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}