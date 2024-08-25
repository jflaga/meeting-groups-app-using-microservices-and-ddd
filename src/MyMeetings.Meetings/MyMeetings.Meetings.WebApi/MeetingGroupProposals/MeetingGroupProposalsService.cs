namespace MyMeetings.Meetings.WebApi.MeetingGroupProposals;

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

    public MeetingGroupProposal? Get(Guid id)
    {
        return meetingGroupProposals.FirstOrDefault(x => x.Id == id);
    }

    public void Accept(Guid id)
    {
        var p = meetingGroupProposals.FirstOrDefault(x => x.Id == id);
        if (p is not null)
        {
            p.StatusCode = "Accepted";
        }
    }
}
