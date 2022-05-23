namespace Tally.Web.Models.Configuration;

public class DiscordBotConfiguration
{
    public ulong ServerId { get; set; }
    public ulong ChannelId { get; set; }
    public string Token { get; set; } = null!;
}