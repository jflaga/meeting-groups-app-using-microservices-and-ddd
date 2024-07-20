using MassTransit;
using MyMeetings.Meetings.IntegrationEvents;

namespace MyMeetings.Administration.WebApi.MeetingGroupProposals;

public class MeetingGroupProposedIntegrationEventConsumer : IConsumer<MeetingGroupProposedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<MeetingGroupProposedIntegrationEvent> context)
    {
        Console.WriteLine($"OrderCreated message: {context.Message.MeetingGroupProposalId}");
    }
}
