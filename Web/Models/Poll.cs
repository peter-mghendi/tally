namespace Web.Models;

public partial class Poll
{
    public int Id { get; set; }
    public PollChannel Channel { get; set; }
    public string Identifier { get; set; } = null!;
    public User Creator { get; set; } = null!;
    public IEnumerable<Option> Options { get; set; } = new List<Option>();
    public IEnumerable<Vote> Votes { get; set; } = new List<Vote>();
}