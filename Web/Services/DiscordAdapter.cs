using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Web.Models.Configuration;

namespace Web.Services;

public class DiscordAdapter : IAsyncDisposable
{
    private readonly DiscordBotConfiguration _discordBotConfig;
    private readonly ILogger<DiscordAdapter> _logger;
    public Lazy<Task<DiscordSocketClient>> LazyClient { get; }

    public DiscordAdapter(DiscordBotConfiguration discordBotConfig, ILogger<DiscordAdapter> logger)
    {
        LazyClient = new Lazy<Task<DiscordSocketClient>>(async () =>
        {
            var client = new DiscordSocketClient();
            await client.LoginAsync(TokenType.Bot, discordBotConfig.Token);
            return client;
        });

        _discordBotConfig = discordBotConfig;
        _logger = logger;
    }

    public async Task<RestUserMessage> CreatePollAsync(string text)
    {
        var client = await LazyClient.Value;
        _logger.LogInformation("Creating poll. Client connection state: {Value}", client.ConnectionState);

        var guild = await client.Rest.GetGuildAsync(_discordBotConfig.ServerId);
        if (guild is null) throw new Exception("Guild is null");
        _logger.LogInformation("Guild name: {Name}", guild.Name);
        
        var channel = await guild.GetTextChannelAsync(_discordBotConfig.ChannelId);
        if (channel is null) throw new Exception("Channel is null");
        _logger.LogInformation("Channel name: {Name}", channel.Name);

        return await channel.SendMessageAsync(text: text);
    }
    
    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("Disposing Discord Gateway Service.");
        var client = await LazyClient.Value;
        await client.DisposeAsync();
    }
}