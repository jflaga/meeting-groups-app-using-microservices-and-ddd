namespace MyMeetings.Administration.WebApi.MeetingGroupProposals;

public class MeetingGroupProposalsService
{
    private static List<MeetingGroupProposal> meetingGroupProposals = new List<MeetingGroupProposal>();

    public void Add(MeetingGroupProposal meetingGroupProposal)
    {
        meetingGroupProposals.Add(meetingGroupProposal);
    }

    public IEnumerable<MeetingGroupProposal> GetAll()
    {
        return meetingGroupProposals;
    }

    public void Add(Guid id)
    {
        meetingGroupProposals.Add(new MeetingGroupProposal
        {
            Id = id,
        });
    }

    public void Accept(Guid id)
    {
        var proposal = meetingGroupProposals.FirstOrDefault(x => x.Id == id);
        proposal.Decision = new MeetingGroupProposalDecision
        {
            Date = DateTime.Now,
            Code = "Accept",
        };
    }
}
