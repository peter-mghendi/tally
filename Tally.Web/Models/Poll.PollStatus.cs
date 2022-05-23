namespace Tally.Web.Models;

public partial class Poll
{
    public enum PollStatus
    {
        Scheduled,
        Ongoing,
        Concluded
    }
}