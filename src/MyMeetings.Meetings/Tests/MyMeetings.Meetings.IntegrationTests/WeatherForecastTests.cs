using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace MyMeetings.Meetings.IntegrationTests;

public class WeatherForecastTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly List<string> _possibleSummaries =
        ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

    [Fact]
    public async Task HappyPathReturnsGoodData()
    {
        var client = factory.CreateClient();

        var forecastResult = await client.GetFromJsonAsync<WeatherForecast[]>("/weatherForecast");

        Assert.Equal(5, forecastResult?.Length);
        foreach (var forecast in forecastResult!)
        {
            Assert.Equal(forecast.TemperatureF, 32 + (int)(forecast.TemperatureC / 0.5556));
            Assert.Contains(forecast.Summary, _possibleSummaries);
        }
    }
}