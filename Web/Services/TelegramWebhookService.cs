using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Web.Models.Configuration;

namespace Web.Services;

public sealed class TelegramWebhookService : IHostedService
{
    private readonly ILogger<TelegramWebhookService> _logger;
    private readonly IServiceProvider _services;
    private readonly TelegramBotConfiguration _telegramBotConfig;

    public TelegramWebhookService(ILogger<TelegramWebhookService> logger,
                            IServiceProvider serviceProvider,
                            IConfiguration configuration)
    {
        _logger = logger;
        _services = serviceProvider;
        _telegramBotConfig = configuration.GetRequiredSection(nameof(TelegramBotConfiguration)).Get<TelegramBotConfiguration>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        // Configure custom endpoint per Telegram API recommendations:
        // REF: https://core.telegram.org/bots/api#setwebhook
        var webhookAddress = @$"{_telegramBotConfig.HostAddress}/webhooks/telegram/{_telegramBotConfig.BotToken}";
        _logger.LogInformation("Setting Telegram webhook: {webhookAddress}", webhookAddress);
        await botClient.SetWebhookAsync(webhookAddress,
            allowedUpdates: Array.Empty<UpdateType>(),
            cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        // Remove webhook upon app shutdown
        _logger.LogInformation("Removing Telegram webhook");
        await botClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
    }
}