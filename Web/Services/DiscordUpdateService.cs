using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Services;

public class DiscordUpdateService
{
    private readonly ILogger<DiscordUpdateService> _logger;
    private readonly TallyContext _context;

    private readonly List<Emoji> _allowedEmoji = new()
    {
        new Emoji("\u0031\uFE0F\u20E3"),
        new Emoji("\u0032\uFE0F\u20E3"),
        new Emoji("\u0033\uFE0F\u20E3"),
        new Emoji("\u0034\uFE0F\u20E3")
    };

    public DiscordUpdateService(ILogger<DiscordUpdateService> logger, TallyContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task CreateVoteAsync(
        Cacheable<IUserMessage, ulong> message,
        Cacheable<IMessageChannel, ulong> channel,
        SocketReaction reaction
    )
    {
        var emote = reaction.Emote;
        var user = reaction.User.Value;
        var userIdentifier = user.Id.ToString();
        var pollIdentifier = reaction.MessageId.ToString();
        var optionIndex = _allowedEmoji.FindIndex(e => e.Name == emote.Name);
        
        var channelPoll = await _context.ChannelPolls
            .Include(cp => cp.Poll)
            .ThenInclude(p => p.Options)
            .Include(cp => cp.Poll)
            .ThenInclude(p => p.LiveVotes)
            .ThenInclude(lv => lv.Option)
            .SingleAsync(cp => cp.PrimaryIdentifier == pollIdentifier && cp.Channel == PollChannel.Discord);
        var poll = channelPoll.Poll;
        var optionId = poll.Options[optionIndex].Id;
        
        if(!reaction.User.IsSpecified)
        {
            _logger.LogInformation("Discarding Discord vote for poll #{Poll}: User is null.", poll);
            return;
        }
        
        if(optionIndex < 0)
        {
            _logger.LogInformation("Discarding Discord vote for poll #{Poll}: Choice is invalid.", poll);
            await message.Value.RemoveReactionAsync(emote, user);
        }
        
        if(poll.EndedAt is not null)
        {
            _logger.LogInformation("Discarding Discord vote for poll {Poll}: Poll has concluded.", poll.Id);
            await message.Value.RemoveReactionAsync(emote, user);
        }
        
        var priorVote = poll.LiveVotes.SingleOrDefault(lv => lv.UserIdentifier == userIdentifier && lv.Channel == PollChannel.Discord && lv.OptionId == optionId);
        if (priorVote is not null)
        {
            _logger.LogInformation("Found prior Discord vote for poll {Poll}: Discarding prior vote.", poll.Id);
                
            // Remove vote from Discord
            var priorOptionIndex = poll.Options.IndexOf(priorVote.Option);
            var messageValue = await message.GetOrDownloadAsync();
            await messageValue.RemoveReactionAsync(_allowedEmoji[priorOptionIndex], user);
            
            // Remove vote from DB
            poll.LiveVotes.Remove(priorVote);
            await _context.SaveChangesAsync();
        }
        
        _logger.LogInformation("Received Discord vote for poll: {Poll}", poll.Id);
        poll.LiveVotes.Add(new LiveVote
        {
            Channel = PollChannel.Discord,
            Option = poll.Options[optionIndex],
            UserIdentifier = userIdentifier
        });
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteVoteAsync(
        Cacheable<IUserMessage, ulong> message,
        Cacheable<IMessageChannel, ulong> channel,
        SocketReaction reaction
    )
    {
        var emote = reaction.Emote;
        var user = reaction.User.Value;
        var userIdentifier = user.Id.ToString();
        var pollIdentifier = reaction.MessageId.ToString();
        var optionIndex = _allowedEmoji.FindIndex(e => e.Name == emote.Name);
        
        var channelPoll = await _context.ChannelPolls
            .Include(cp => cp.Poll)
            .ThenInclude(p => p.Options)
            .Include(cp => cp.Poll)
            .ThenInclude(p => p.LiveVotes)
            .ThenInclude(lv => lv.Option)
            .SingleAsync(cp => cp.PrimaryIdentifier == pollIdentifier && cp.Channel == PollChannel.Discord);
        var poll = channelPoll.Poll;
        var optionId = poll.Options[optionIndex].Id;
        
        _logger.LogInformation("Received Discord vote removal for poll: {Poll}", 0);
        
        var priorVote = poll.LiveVotes.SingleOrDefault(lv => lv.UserIdentifier == userIdentifier && lv.Channel == PollChannel.Discord && lv.OptionId == optionId);
        if (priorVote is not null)
        {
            poll.LiveVotes.Remove(priorVote);
            await _context.SaveChangesAsync();
        }
    }
}