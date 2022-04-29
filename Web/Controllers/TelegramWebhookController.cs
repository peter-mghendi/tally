using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Web.Services;

namespace Web.Controllers;

public class TelegramWebhookController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromServices] TelegramUpdateService telegramUpdateService,
        [FromBody] Update update)
    {
        await telegramUpdateService.HandleAsync(update);
        return Ok();
    }
}