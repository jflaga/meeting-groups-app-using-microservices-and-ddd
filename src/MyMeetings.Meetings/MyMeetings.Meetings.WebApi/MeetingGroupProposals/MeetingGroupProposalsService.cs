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
}
