using MassTransit;
using MyMeetings.Meetings.IntegrationEvents;

namespace MyMeetings.Administration.WebApi.MeetingGroupProposals;

public class MeetingGroupProposedIntegrationEventConsumer : IConsumer<MeetingGroupProposedIntegrationEvent>
{
    private readonly MeetingGroupProposalsService service;

    public MeetingGroupProposedIntegrationEventConsumer(MeetingGroupProposalsService service)
    {
        this.service = service;
    }

    public async Task Consume(ConsumeContext<MeetingGroupProposedIntegrationEvent> context)
    {
        service.Add(context.Message.MeetingGroupProposalId);
        Console.WriteLine($"OrderCreated message: {context.Message.MeetingGroupProposalId}");
    }
}
