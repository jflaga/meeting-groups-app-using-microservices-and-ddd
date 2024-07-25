using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using MyMeetings.Meetings.WebApi.MeetingGroupProposals;
using System.Net;
using System.Text.Json;

namespace MyMeetings.Meetings.IntegrationTests.MeetingGroupProposals;

public class ProposeMeetingGroupTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonWebOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task CreateMeetingGroupProposal_Succeeds()
    {
        var payload = new MeetingGroupProposalInputDto
        {
            Name = "Meeting Group 1",
            LocationCity = "The City",
            LocationCountryCode = "Better Country",
        };
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/meetings/MeetingGroupProposals", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<MeetingGroupProposal>(responseContent, JsonWebOptions);
        Assert.NotNull(result);
        Assert.Equal(payload.Name, result.Name);
        Assert.Equal(payload.LocationCity, result.LocationCity);
        Assert.Equal(payload.LocationCountryCode, result.LocationCountryCode);
        Assert.NotEqual(DateTimeOffset.MinValue, result.ProposalDate);
        Assert.NotEqual(Guid.Empty, result.ProposalUserId);
        Assert.Equal("InVerification", result.StatusCode);
    }
}
