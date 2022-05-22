using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Web.Channels;
using Web.Data;
using Web.Models;

namespace Web.Hubs;

[Authorize]
public class TallyHub : Hub<TallyHub.IClient>
{
    private readonly ChannelWrapper _channels;
    private readonly TallyContext _context;

    public TallyHub(ChannelWrapper channels, TallyContext context)
    {
        _channels = channels;
        _context = context;
    }

    public interface IClient
    {
        public Task AcknowledgeSubscription(int pollId);
        public Task UpdateResults(Dictionary<string, ChannelResult> results);
        public Task UpdateResult(string channel, ChannelResult results);
    }

    public async Task Refresh(int pollId)
    {
        var poll = await _context.Polls
            .Include(p => p.ChannelPolls)   
            .Include(p => p.Options)
            .SingleAsync(p => p.Id == pollId);

        var results = new Dictionary<string, ChannelResult>();

        foreach (var channelPoll in poll.ChannelPolls)
        {
            var pollChannel = channelPoll.Channel;
            var channel = _channels.Resolve(pollChannel);
            results.Add(pollChannel.ToString(), await  channel.CountVotesAsync(channelPoll));
        }

        await Clients.Caller.UpdateResults(results);
    }

    public async Task Subscribe(int pollId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, pollId.ToString());
        await Clients.Caller.AcknowledgeSubscription(pollId);
    }
}