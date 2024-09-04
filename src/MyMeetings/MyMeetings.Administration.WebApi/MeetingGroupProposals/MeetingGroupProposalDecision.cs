namespace MyMeetings.Administration.WebApi.MeetingGroupProposals;

public class MeetingGroupProposalDecision
{
    public DateTimeOffset? Date { get; set; }

    public string Code { get; set; }

    public string RejectReason { get; set; }

    private bool IsAccepted => this.Code == "Accept";

    private bool IsRejected => this.Code == "Reject";

}
