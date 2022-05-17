using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Web.Models.Configuration;

namespace Web.Services;

public sealed class DiscordGatewayService : IHostedService, IAsyncDisposable
{
    private readonly DiscordBotConfiguration _discordBotConfig;
    private readonly DiscordUpdateService _updateService;
    private readonly Lazy<Task<DiscordSocketClient>> _lazyClient;
    private readonly ILogger<DiscordGatewayService> _logger;

    public DiscordGatewayService(
        DiscordBotConfiguration discordBotConfig,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<DiscordGatewayService> logger
    )
    {
        using var scope = serviceScopeFactory.CreateScope();
        _updateService = scope.ServiceProvider.GetRequiredService<DiscordUpdateService>();
        _lazyClient = new Lazy<Task<DiscordSocketClient>>(async () =>
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
        var client = await _lazyClient.Value;
        _logger.LogInformation("Creating poll. Client connection state: {Value}", client.ConnectionState);

        var guild = await client.Rest.GetGuildAsync(_discordBotConfig.ServerId);
        if (guild is null) throw new Exception("Guild is null");
        _logger.LogInformation("Guild name: {Name}", guild.Name);
        
        var channel = await guild.GetTextChannelAsync(_discordBotConfig.ChannelId);
        if (channel is null) throw new Exception("Channel is null");
        _logger.LogInformation("Channel name: {Name}", channel.Name);

        return await channel.SendMessageAsync(text: text);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Discord Gateway Service.");

        var client = await _lazyClient.Value;
        client.Log += Log;
        client.ReactionAdded += _updateService.CreateVoteAsync;
        client.ReactionRemoved += _updateService.DeleteVoteAsync;
        
        await client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Discord Gateway Service.");

        var client = await _lazyClient.Value;
        client.Log -= Log;
        client.ReactionAdded -= _updateService.CreateVoteAsync;
        client.ReactionRemoved -= _updateService.DeleteVoteAsync;
        
        await client.LogoutAsync();
        await client.StopAsync();
    }

    private Task Log(LogMessage message)
    {
        _logger.LogInformation("Discord: {Message}", message);
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("Disposing Discord Gateway Service.");
        var client = await _lazyClient.Value;
        await client.DisposeAsync();
    }
}