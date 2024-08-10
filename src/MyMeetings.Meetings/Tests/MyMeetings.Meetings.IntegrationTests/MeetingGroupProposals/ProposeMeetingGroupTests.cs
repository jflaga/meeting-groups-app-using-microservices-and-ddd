using DotNet.Testcontainers.Builders;
using MyMeetings.Meetings.IntegrationTests._Utils;
using MyMeetings.Meetings.WebApi.MeetingGroupProposals;
using System.Net;
using System.Text.Json;
using Testcontainers.RabbitMq;
using MassTransit.Testing;
using MyMeetings.Meetings.IntegrationEvents;

namespace MyMeetings.Meetings.IntegrationTests.MeetingGroupProposals;

public class ProposeMeetingGroupTests : IClassFixture<MyMeetingsWebApplicationFactory<Program>>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonWebOptions = new(JsonSerializerDefaults.Web);
    private readonly MyMeetingsWebApplicationFactory<Program> factory;

    private readonly RabbitMqContainer rabbitMqContainer;
    private readonly ITestHarness MassTransitTestHarness;

    public ProposeMeetingGroupTests(MyMeetingsWebApplicationFactory<Program> factory)
    {
        this.factory = factory;

        rabbitMqContainer = new RabbitMqBuilder()
            //.WithImage("rabbitmq:3-management-alpine")
            //.WithPortBinding(5673)
            ////.WithEnvironment("RABBITMQ_DEFAULT_USER", RabbitMqUsername)
            ////.WithEnvironment("RABBITMQ_DEFAULT_PASS", RabbitMqPassword)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(5672)
                //.UntilPortIsAvailable(15672) // 15672: RabbitMQ management port            
            ).Build();

        MassTransitTestHarness = factory.Services.GetTestHarness();

    }

    public async Task InitializeAsync()
    {
        await rabbitMqContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await rabbitMqContainer.DisposeAsync();
    }

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

        Assert.True(await MassTransitTestHarness.Published.Any<MeetingGroupProposedIntegrationEvent>(
            x => x.Context.Message.MeetingGroupProposalId == result.Id));

    }

}
