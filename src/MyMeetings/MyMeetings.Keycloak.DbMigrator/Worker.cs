namespace MyMeetings.Keycloak.DbMigrator;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> logger;
    private readonly KeycloakDataSeeder keycloakDataSeeder;

    public Worker(ILogger<Worker> logger, KeycloakDataSeeder keycloakDataSeeder)
    {
        this.logger = logger;
        this.keycloakDataSeeder = keycloakDataSeeder;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //while (!stoppingToken.IsCancellationRequested)
        //{
        //    if (_logger.IsEnabled(LogLevel.Information))
        //    {
        //        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        //    }
        //    await Task.Delay(1000, stoppingToken);
        //}

        await keycloakDataSeeder.SeedAsync();
    }
}
