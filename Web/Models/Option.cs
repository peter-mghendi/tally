using Newtonsoft.Json;

namespace Web.Models;

public class Option
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public Poll Poll { get; set; } = null!;
    
    [JsonIgnore]
    public IList<CachedVote> CachedVotes { get; set; } = new List<CachedVote>();
    
    [JsonIgnore]
    public IList<LiveVote> LiveVotes { get; set; } = new List<LiveVote>();
}