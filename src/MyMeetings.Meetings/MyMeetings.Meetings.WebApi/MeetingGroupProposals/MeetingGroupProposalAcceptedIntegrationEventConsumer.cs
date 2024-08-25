using MassTransit;
using MyMeetings.Administration.WebApi.MeetingGroupProposals;

namespace MyMeetings.Meetings.WebApi.MeetingGroupProposals;

public class MeetingGroupProposalAcceptedIntegrationEventConsumer : IConsumer<MeetingGroupProposalAcceptedIntegrationEvent>
{
    private readonly MeetingGroupProposalsService service;

    public MeetingGroupProposalAcceptedIntegrationEventConsumer(MeetingGroupProposalsService service)
    {
        this.service = service;
    }

    public async Task Consume(ConsumeContext<MeetingGroupProposalAcceptedIntegrationEvent> context)
    {
        service.Accept(context.Message.MeetingGroupProposalId);
    }
}
