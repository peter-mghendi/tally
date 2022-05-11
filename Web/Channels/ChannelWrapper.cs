using Web.Models;

namespace Web.Channels;

// TODO: Source-generate some or all of this
public class ChannelWrapper
{
    private readonly Dictionary<PollChannel, IChannel> _map;
    
    public TelegramChannel Telegram { get; private init; }
    public TwitterChannel Twitter { get; private init; }
    public GitHubChannel GitHub { get; private init; }  

    public ChannelWrapper(TelegramChannel telegramChannel, TwitterChannel twitterChannel, GitHubChannel gitHubChannel)
    {
        Telegram = telegramChannel;
        Twitter = twitterChannel;
        GitHub = gitHubChannel;
        
        _map = new Dictionary<PollChannel, IChannel>
        {
            [PollChannel.Telegram] = Telegram,
            [PollChannel.Twitter] = Twitter,
            [PollChannel.GitHub] = GitHub,
        };
    }

    public IChannel Resolve(PollChannel channel) => _map[channel];
}