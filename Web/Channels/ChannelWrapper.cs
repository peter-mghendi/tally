namespace Web.Channels;

public class ChannelWrapper
{
    public IChannel Telegram { get; private init; }
    public IChannel Twitter { get; private init; }
    public IChannel GitHub { get; private init; }

    public ChannelWrapper(TelegramChannel telegramChannel, TwitterChannel twitterChannel, GitHubChannel gitHubChannel)
    {
        Telegram = telegramChannel;
        Twitter = twitterChannel;
        GitHub = gitHubChannel;
    }
}