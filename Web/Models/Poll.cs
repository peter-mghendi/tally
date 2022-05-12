using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Web.Models;

public partial class Poll
{
    public int Id { get; set; }
    
    public string Question { get; set; } = string.Empty;
    
    [Display(Name = "Started At")]
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    
    [Display(Name = "Ended At")]
    [DisplayFormat(NullDisplayText = nameof(PollStatus.Ongoing))]
    public DateTime? EndedAt { get; set; }
    
    public User Creator { get; set; } = null!;
    
    [Display(Name = "Channels")]
    public ICollection<ChannelPoll> ChannelPolls { get; set; } = new List<ChannelPoll>();
    
    public IList<Option> Options { get; set; } = new List<Option>();
    
    [JsonIgnore]
    public ICollection<LiveVote> LiveVotes { get; set; } = new List<LiveVote>();
    
    [JsonIgnore]
    public ICollection<CachedVote> CachedVotes { get; set; } = new List<CachedVote>();

    public PollStatus Status => StartedAt > DateTime.UtcNow ? PollStatus.Scheduled
        : EndedAt < DateTime.UtcNow ? PollStatus.Concluded
        : PollStatus.Ongoing;
}