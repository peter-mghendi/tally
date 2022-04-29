namespace Web.Models.Configuration;

public class TelegramBotConfiguration
{
    public long ChatId { get; set; }
    public string BotToken { get; init; } = null!;
    public string HostAddress { get; init; } = null!;
}