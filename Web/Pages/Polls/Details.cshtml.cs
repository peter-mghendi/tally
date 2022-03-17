using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Web.Channels;
using Web.Data;
using Web.Models;

namespace Web.Pages.Polls;

[Authorize]
public class Details : PageModel
{
    private readonly ILogger<Create> _logger;
    private readonly TallyContext _context;
    private readonly IChannel _telegramChannel;
    private readonly IChannel _twitterChannel;
    private readonly UserManager<User> _userManager;

    [BindProperty(SupportsGet = true)] 
    public int Id { get; set; }
    
    [BindProperty] 
    public Poll Poll { get; set; }
    
    [BindProperty] 
    public Dictionary<string, ChannelResult> Results { get; set; }

    public Details(
        ILogger<Create> logger,
        TallyContext context,
        ChannelWrapper channels,
        UserManager<User> userManager
    )
    {
        _logger = logger;
        _context = context;
        _telegramChannel = channels.Telegram;
        _twitterChannel = channels.Twitter;
        _userManager = userManager;

        Poll = new();
    }

    public async Task OnGetAsync()
    {
        Poll = await _context.Polls
            .Include(p => p.ChannelPolls)
            .Include(p => p.Options)
            .SingleAsync(p => p.Id == Id);

        var telegramPoll = Poll.ChannelPolls.Single(cp => cp.Channel == PollChannel.Telegram);
        var twitterPoll = Poll.ChannelPolls.Single(cp => cp.Channel == PollChannel.Twitter);

        Results = new Dictionary<string, ChannelResult>
        {
            [nameof(PollChannel.Telegram)] = await _telegramChannel.CountVotesAsync(telegramPoll),
            [nameof(PollChannel.Twitter)] = await _twitterChannel.CountVotesAsync(twitterPoll),
        };

        _logger.LogInformation(JsonSerializer.Serialize(Results, new JsonSerializerOptions {WriteIndented = true}));
    }
}