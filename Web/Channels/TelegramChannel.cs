using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Web.Data;
using Web.Models;
using static Web.Models.Poll;

namespace Web.Channels;

public class TelegramChannel
{
    private const string TelegramBotTokenKey = nameof(TelegramBotTokenKey);
    private const string TelegramChatIdKey = nameof(TelegramChatIdKey);

    private readonly TallyContext _context;
    private readonly UserManager<User> _userManager;

    public TelegramChannel(IConfiguration configuration, TallyContext context, UserManager<User> userManager)
    {
        Console.WriteLine(JsonSerializer.Serialize(configuration, new JsonSerializerOptions {WriteIndented = true}));
        _context = context;
        _userManager = userManager;
    }

    public async Task CreatePollAsync(string question, string[] options, string creatorId,
        CancellationToken cancellationToken = default)
    {
        // TODO: Do the Telegram stuff   
        var creator = await _userManager.FindByIdAsync(creatorId);
        var poll = new Poll
        {
            Channel = PollChannel.Telegram, 
            Identifier = string.Empty, 
            Creator = creator!,
            Options = options.Select(o => new Option { Text = o }).ToList()
        };

        await _context.Polls.AddAsync(poll, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RecordVoteAsync(int pollId, int choiceId, string voterId, CancellationToken cancellationToken = default)
    {
        // TODO: Do the Telegram stuff 
        var voter = await _userManager.FindByIdAsync(voterId);
        var poll = await _context.Polls.FindAsync(pollId, cancellationToken);
        var option = await _context.Options.FindAsync(choiceId, cancellationToken);
        poll!.Votes.Add(new() { Option = option!, User = voter });
        await _context.SaveChangesAsync(cancellationToken);
    }
}