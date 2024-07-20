namespace MyMeetings.Meetings.IntegrationEvents;

public class MeetingGroupProposedIntegrationEvent
{
    public Guid MeetingGroupProposalId { get; set; }
    public string Name { get; set; }
    //public string Description { get; set; }
    public string LocationCity { get; set; }
    public string LocationCountryCode { get; set; }
    public Guid ProposalUserId { get; set; }
    public DateTimeOffset ProposalDate { get; set; }
}
