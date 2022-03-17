namespace Web.Models;

public class Option
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public Poll Poll { get; set; } = null!;
    public IList<CachedVote> CachedVotes { get; set; } = new List<CachedVote>();
    public IList<LiveVote> LiveVotes { get; set; } = new List<LiveVote>();
}