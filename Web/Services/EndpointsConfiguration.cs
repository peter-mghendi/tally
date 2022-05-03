using Microsoft.AspNetCore.Mvc;
using Octokit.Webhooks.AspNetCore;
using Telegram.Bot.Types;
using Web.Models.Configuration;

namespace Web.Services;

public static class EndpointsConfiguration
{
    public static void MapTelegramWebHooks(this IEndpointRouteBuilder endpoints, TelegramBotConfiguration telegramBotConfig)
    {
        // Configure custom endpoint per Telegram API recommendations:
        // REF: https://core.telegram.org/bots/api#setwebhook
        async Task Handler([FromServices] TelegramUpdateService telegramUpdateService, [FromBody] Update update, CancellationToken cancellationToken) 
            => await telegramUpdateService.HandleAsync(update, cancellationToken);
        endpoints.MapPost($"webhooks/telegram/{telegramBotConfig.BotToken}", (Func<TelegramUpdateService, Update, CancellationToken, Task>) Handler).WithName("webhooks.telegram");
    }
    public static void MapGitHubWebHooks(this IEndpointRouteBuilder endpoints, GitHubBotConfiguration gitHubBotConfig)
    {
        endpoints.MapGitHubWebhooks("/webhooks/github", gitHubBotConfig.WebHookSecret);
    }
}