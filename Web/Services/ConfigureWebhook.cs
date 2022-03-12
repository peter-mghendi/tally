using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Web.Models.Configuration;

namespace Web.Services;

public class ConfigureWebhook : IHostedService
{
    private readonly ILogger<ConfigureWebhook> _logger;
    private readonly IServiceProvider _services;
    private readonly TelegramBotConfiguration _telegramBotConfig;

    public ConfigureWebhook(ILogger<ConfigureWebhook> logger,
                            IServiceProvider serviceProvider,
                            IConfiguration configuration)
    {
        _logger = logger;
        _services = serviceProvider;
        _telegramBotConfig = configuration.GetSection("TelegramBotConfiguration").Get<TelegramBotConfiguration>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        // Configure custom endpoint per Telegram API recommendations:
        // REF: https://core.telegram.org/bots/api#setwebhook
        var webhookAddress = @$"{_telegramBotConfig.HostAddress}/bot/{_telegramBotConfig.BotToken}";
        _logger.LogInformation("Setting webhook: {webhookAddress}", webhookAddress);
        await botClient.SetWebhookAsync(webhookAddress,
            allowedUpdates: Array.Empty<UpdateType>(),
            cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        // Remove webhook upon app shutdown
        _logger.LogInformation("Removing webhook");
        await botClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
    }
}