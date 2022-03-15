namespace Web.Models;

public partial class Poll
{
    public enum PollStatus
    {
        Scheduled,
        Happening,
        Concluded
    }
}