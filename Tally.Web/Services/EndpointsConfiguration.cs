using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Octokit.Webhooks.AspNetCore;
using Tally.Web.Models.Configuration;
using Telegram.Bot.Types;

namespace Tally.Web.Services;

public static class EndpointsConfiguration
{
    public static void MapTelegramWebHooks(this IEndpointRouteBuilder endpoints,
        TelegramBotConfiguration telegramBotConfig)
    {
        // Configure custom endpoint per Telegram API recommendations:
        // REF: https://core.telegram.org/bots/api#setwebhook
        async Task Handler(
            [FromServices] TelegramUpdateService telegramUpdateService,
            HttpContext context,
            CancellationToken cancellationToken
        )
        {
            var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            var update = JsonConvert.DeserializeObject<Update>(body)!;
            await telegramUpdateService.HandleAsync(update, cancellationToken);
        }

        endpoints.MapPost($"webhooks/telegram/{telegramBotConfig.BotToken}", Handler).WithName("webhooks.telegram");
    }

    public static void MapGitHubWebHooks(this IEndpointRouteBuilder endpoints, GitHubBotConfiguration gitHubBotConfig)
    {
        endpoints.MapGitHubWebhooks("/webhooks/github", gitHubBotConfig.WebHookSecret);
    }
}