namespace Tally.Web.Models;

public class LiveVote
{
    public int Id { get; set; }
    public string UserIdentifier { get; set; } = String.Empty;
    public PollChannel Channel { get; set; }
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    public Option Option { get; set; } = null!;
    public Poll Poll { get; set; } = null!;
    public int OptionId { get; set; }
}