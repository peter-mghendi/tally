using System.Text;
using Web.Models;
using Web.Services;

namespace Web.Channels;

public class DiscordChannel : Channel
{
    private readonly DiscordGatewayService _gatewayService;

    private readonly string[] _reactions = {":one:", ":two:", ":three:", ":four:"};
    
    public DiscordChannel(DiscordGatewayService gatewayService)
    {
        _gatewayService = gatewayService;
    }
    
    public override PollChannel PollChannel => PollChannel.Discord;
    public override async Task<ChannelPoll> CreatePollAsync(string question, IEnumerable<string> options, CancellationToken cancellationToken = default)
    {
        var textBuilder = new StringBuilder($"{question}\n\nReact with one of the following to vote:\n");
        var enumerable = options as string[] ?? options.ToArray();
        for (var i = 0; i < enumerable.Length; i++)
        {
            textBuilder.Append($"{_reactions[i]} {enumerable.ElementAt(i)}\n");
        }

        var message = await _gatewayService.CreatePollAsync(text: textBuilder.ToString());
        var identifier = message.Id.ToString();
        return BuildPoll(identifier, identifier);
    }

    public override Task<ChannelResult> CountVotesAsync(ChannelPoll channelPoll, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task ConcludePollAsync(ChannelPoll channelPoll, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task DeletePollAsync(ChannelPoll channelPoll, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}