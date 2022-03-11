namespace Web.Models;

public class Poll
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User Creator { get; set; } = null!;
    public ICollection<ChannelPoll> ChannelPolls { get; set; } = new List<ChannelPoll>();
    public IList<Option> Options { get; set; } = new List<Option>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}