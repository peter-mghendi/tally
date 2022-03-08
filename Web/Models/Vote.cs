namespace Web.Models;

public class Vote
{
    public int Id { get; set; }
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    public Option Option { get; set; } = null!;
    public Poll Poll { get; set; } = null!;
    public User User { get; set; } = null!;
}