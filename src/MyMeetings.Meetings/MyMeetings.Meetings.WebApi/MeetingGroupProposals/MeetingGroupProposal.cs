namespace MyMeetings.Meetings.WebApi.MeetingGroupProposals;

public class MeetingGroupProposal
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string LocationCity { get; set; }
    public string LocationCountryCode { get; set; }
    public Guid ProposalUserId { get; set; }
    public DateTimeOffset ProposalDate { get; set; }
    public string StatusCode { get; set; }
}
