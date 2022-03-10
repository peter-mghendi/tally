namespace Web.Models;

public partial class ChannelPoll
{
    public int Id { get; set; }
    public PollChannel Channel { get; set; }
    public string Identifier { get; set; } = null!;
    public Poll Poll { get; set; }
}