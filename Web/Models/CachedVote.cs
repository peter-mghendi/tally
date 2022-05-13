namespace Web.Models;

public class CachedVote
{
    public int Id { get; set; }
    public int Count { get; set; }
    public PollChannel Channel { get; set; }
    public DateTime LastRefreshedAt { get; set; } = DateTime.UtcNow;
    public Option Option { get; set; } = null!;
    public Poll Poll { get; set; } = null!;
    public int OptionId { get; set; }
}