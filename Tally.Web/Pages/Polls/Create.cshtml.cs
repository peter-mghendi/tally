using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tally.Web.Channels;
using Tally.Web.Data;
using Tally.Web.Models;

namespace Tally.Web.Pages.Polls;

[Authorize]
public class Create : PageModel
{
    private readonly List<IChannel> _channels;
    private readonly TallyContext _context;
    private readonly ILogger<Create> _logger;
    private readonly UserManager<User> _userManager;

    public Create(
        ILogger<Create> logger,
        TallyContext context,
        ChannelWrapper channels,
        UserManager<User> userManager
    )
    {
        _logger = logger;
        _context = context;
        _channels = new List<IChannel>
        {
            channels.Telegram,
            channels.Twitter,
            channels.GitHub,
            channels.Discord,
            channels.Web
        };
        _userManager = userManager;

        Poll = new Poll();
    }

    [BindProperty] public Poll Poll { get; set; }

    public IActionResult OnGet()
    {
        Poll.Options = Enumerable.Repeat(new Option(), 4).ToList();
        return Page();
    }

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public async Task<IActionResult> OnPostAsync()
    {
        var options = Poll.Options.Select(o => o.Text);
        var tasks = _channels.Select(c => c.CreatePollAsync(Poll.Question, options));

        Poll.Creator = await _userManager.GetUserAsync(User) ?? throw new InvalidOperationException();
        Poll.ChannelPolls = await Task.WhenAll(tasks);
        
        foreach (var cp in Poll.ChannelPolls)
        {
            _logger.LogInformation("Primary: {Primary}, Auxiliary: {SecondaryIdentifier}", cp.PrimaryIdentifier, cp.AuxiliaryIdentifier);
        }

        await _context.Polls.AddAsync(Poll);
        await _context.SaveChangesAsync();

        var cachedPollChannels = new List<PollChannel> { PollChannel.Twitter };
        var cachedChannelPolls = Poll.ChannelPolls.Where(cp => cachedPollChannels.Contains(cp.Channel));

        var cachedVotes = new List<CachedVote>();
        foreach (var channelPoll in cachedChannelPolls)
        {
            var voteCounts = Poll.Options.Select((option, _) => new CachedVote
            {
                Count = 0,
                Channel = channelPoll.Channel,
                Option = option,
                Poll = Poll
            });
            cachedVotes.AddRange(voteCounts);
        }

        await _context.CachedVotes.AddRangeAsync(cachedVotes);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}