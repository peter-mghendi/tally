using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public partial class Poll
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    [DisplayFormat(NullDisplayText = nameof(PollStatus.Happening))]
    public DateTime? EndedAt { get; set; }
    public User Creator { get; set; } = null!;
    public ICollection<ChannelPoll> ChannelPolls { get; set; } = new List<ChannelPoll>();
    public IList<Option> Options { get; set; } = new List<Option>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();

    public PollStatus Status => StartedAt > DateTime.UtcNow ? PollStatus.Scheduled
        : EndedAt < DateTime.UtcNow ? PollStatus.Concluded
        : PollStatus.Happening;
}