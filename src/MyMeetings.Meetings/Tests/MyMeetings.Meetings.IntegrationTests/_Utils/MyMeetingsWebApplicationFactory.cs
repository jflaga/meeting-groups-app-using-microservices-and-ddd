using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Testcontainers.RabbitMq;

namespace MyMeetings.Meetings.IntegrationTests._Utils;

public class MyMeetingsWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly RabbitMqContainer rabbitMqContainer;

    public ITestHarness MassTransitTestHarness { get; private set; } = null!;

    public MyMeetingsWebApplicationFactory()
    {
        rabbitMqContainer = new RabbitMqBuilder()
            //.WithImage("rabbitmq:3-management-alpine")
            //.WithPortBinding(5673)
            ////.WithEnvironment("RABBITMQ_DEFAULT_USER", RabbitMqUsername)
            ////.WithEnvironment("RABBITMQ_DEFAULT_PASS", RabbitMqPassword)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(5672)
                //.UntilPortIsAvailable(15672) // 15672: RabbitMQ management port            
            ).Build();

        MassTransitTestHarness = this.Services.GetTestHarness();
    }

    public async Task InitializeAsync()
    {
        await rabbitMqContainer.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        //builder.ConfigureAppConfiguration(delegate (WebHostBuilderContext _, IConfigurationBuilder configBuilder)
        //{
        //    string path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.IntegrationTests.json");
        //    configBuilder.AddJsonFile(path);
        //});

        //builder.ConfigureTestServices(services =>
        //{
        //    services.RemoveMassTransitHostedService();
        //});

        builder.ConfigureServices(services =>
        {
            services.AddMassTransitTestHarness();
            services.AddSingleton<IStartupFilter>(new AutoAuthorizeStartupFilter());
        });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await rabbitMqContainer.DisposeAsync();
    }
}
