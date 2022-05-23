using Discord;

namespace Tally.Web.Services;

public sealed class DiscordGatewayService : IHostedService
{
    private readonly DiscordAdapter _adapter;
    private readonly DiscordUpdateService _updateService;
    private readonly ILogger<DiscordGatewayService> _logger;

    public DiscordGatewayService(
        DiscordAdapter adapter,
        IServiceScopeFactory services,
        ILogger<DiscordGatewayService> logger
    )
    {
        var scope = services.CreateScope();
        _updateService = scope.ServiceProvider.GetRequiredService<DiscordUpdateService>();

        _adapter = adapter;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Discord Gateway Service.");

        var client = await _adapter.LazyClient.Value;
        client.Log += Log;
        client.ReactionAdded += _updateService.CreateVoteAsync;
        client.ReactionRemoved += _updateService.DeleteVoteAsync;
        
        await client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Discord Gateway Service.");

        var client = await _adapter.LazyClient.Value;
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
}