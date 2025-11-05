using Microsoft.Extensions.Hosting;
using Raven.Migrations;

public class MigrationService(MigrationRunner runner, IHostApplicationLifetime applicationLifetime) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Run all the migrations
        runner.Run();
        if (cancellationToken.IsCancellationRequested)
            return;

        // Stop the application rather than waiting for Ctrl+C from the user
        applicationLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}