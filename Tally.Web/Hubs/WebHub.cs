using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Tally.Web.Channels;
using Tally.Web.Data;
using Tally.Web.Models;

namespace Tally.Web.Hubs;

[Authorize]
public class WebHub : Hub<WebHub.IClient>
{
    private readonly IHubContext<TallyHub, TallyHub.IClient> _hubContext;
    private readonly TallyContext _context;
    private readonly WebChannel _channel;

    public WebHub(IHubContext<TallyHub, TallyHub.IClient> hubContext, WebChannel channel, TallyContext context)
    {
        _channel = channel;
        _context = context;
        _hubContext = hubContext;
    }

    public interface IClient
    {
        public Task AcknowledgeSubscription(int pollId);
        public Task AcknowledgeVote(int optionId);
        public Task AcknowledgeRetractVote();
        public Task UpdateResults(double[] results);
    }

    public async Task Vote(int pollId, int optionId)
    {
        var poll = await GetPollAsync(pollId);
        var channelPoll = poll.ChannelPolls.Single(p => p.Channel == PollChannel.Web);

        var option = poll.Options.Single(o => o.Id == optionId);
        var vote = new LiveVote
        {
            Poll = poll,
            Channel = PollChannel.Web,
            Option = option,
            UserIdentifier = Context.UserIdentifier!
        };

        await _context.LiveVotes.AddAsync(vote);
        await _context.SaveChangesAsync();
        await Clients.Caller.AcknowledgeVote(optionId);
        await Clients.Group(pollId.ToString()).UpdateResults(GetResults(poll));
        await _hubContext.Clients.Group(poll.Id.ToString())
            .UpdateResult(nameof(PollChannel.Web), await _channel.CountVotesAsync(channelPoll));
    }

    public async Task RetractVote(int pollId, int optionId)
    {
        var poll = await GetPollAsync(pollId);
        var channelPoll = poll.ChannelPolls.Single(p => p.Channel == PollChannel.Web);
        
        var vote = poll.LiveVotes
            .Single(v =>
                v.Channel == PollChannel.Web && v.OptionId == optionId && v.UserIdentifier == Context.UserIdentifier);

        _context.LiveVotes.Remove(vote);
        await _context.SaveChangesAsync();
        await Clients.Caller.AcknowledgeRetractVote();
        await Clients.Group(pollId.ToString()).UpdateResults(GetResults(poll));
        await _hubContext.Clients.Group(poll.Id.ToString())
            .UpdateResult(nameof(PollChannel.Web), await _channel.CountVotesAsync(channelPoll));
    }

    public async Task Subscribe(int pollId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, pollId.ToString());
        await Clients.Caller.AcknowledgeSubscription(pollId);
    }

    private async Task<Poll> GetPollAsync(int pollId)
    {
        return await _context.Polls
            .Include(p => p.ChannelPolls)
            .Include(p => p.Options)
            .ThenInclude(o => o.LiveVotes)
            .Include(p => p.LiveVotes)
            .SingleAsync(p => p.Id == pollId);
    }
    
    private static double[] GetResults(Poll poll)
    {
        var votes = poll.Options.Select(option => (double) option.LiveVotes.Count).ToList();
        var sum = votes.Sum();

        return sum == 0 ? new[] {0d, 0d, 0d, 0d} : votes.Select(v => (v / sum) * 100).ToArray();
    }
}