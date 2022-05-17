using Discord;
using Discord.WebSocket;

namespace Web.Services;

public class DiscordUpdateService
{
    private readonly ILogger<DiscordUpdateService> _logger;

    public DiscordUpdateService(ILogger<DiscordUpdateService> logger)
    {
        _logger = logger;
    }

    public async Task CreateVoteAsync(
        Cacheable<IUserMessage, ulong> message,
        Cacheable<IMessageChannel, ulong> channel,
        SocketReaction reaction
    )
    {
        _logger.LogInformation("Received Discord vote for poll: {Poll}", 0);
        await Task.Delay(TimeSpan.Zero);
    }
    
    public async Task DeleteVoteAsync(
        Cacheable<IUserMessage, ulong> message,
        Cacheable<IMessageChannel, ulong> channel,
        SocketReaction reaction
    )
    {
        _logger.LogInformation("Received Discord vote removal for poll: {Poll}", 0);
        await Task.Delay(TimeSpan.Zero);
    }
}