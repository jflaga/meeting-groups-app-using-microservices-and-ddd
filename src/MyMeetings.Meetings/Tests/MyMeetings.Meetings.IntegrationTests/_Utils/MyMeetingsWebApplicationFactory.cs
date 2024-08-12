using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Testcontainers.RabbitMq;

namespace MyMeetings.Meetings.IntegrationTests._Utils;

public class MyMeetingsWebApplicationFactory<T> : WebApplicationFactory<T> where T : class
{
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
}
