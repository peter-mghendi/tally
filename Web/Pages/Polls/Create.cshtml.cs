using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Web.Channels;
using Web.Data;
using Web.Models;
using User = Web.Models.User;

namespace Web.Pages.Polls;

[Authorize]
public class Create : PageModel
{
    private readonly ILogger<Create> _logger;
    private readonly TallyContext _context;
    private readonly List<IChannel> _channels;
    private readonly UserManager<User> _userManager;

    [BindProperty] 
    public Poll Poll { get; set; }

    public Create(
        ILogger<Create> logger,
        TallyContext context,
        ChannelWrapper channels,
        UserManager<User> userManager
    )
    {
        _logger = logger;
        _context = context;
        _channels = new List<IChannel>()
        {
            channels.Telegram, 
            channels.Twitter, 
            channels.GitHub,
            channels.Discord
        };
        _userManager = userManager;
        
        Poll = new Poll();
    }

    public IActionResult OnGet()
    {
        Poll.Options = Enumerable.Repeat(new Option(), 4).ToList();
        return Page();
    }

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public async Task<IActionResult> OnPostAsync()
    {
        Poll.Creator = await _userManager.GetUserAsync(User);
        Poll.ChannelPolls = new List<ChannelPoll>
        {
            await _channels[0].CreatePollAsync(Poll.Question, Poll.Options.Select(o => o.Text)),
            await _channels[1].CreatePollAsync(Poll.Question, Poll.Options.Select(o => o.Text)),
            await _channels[2].CreatePollAsync(Poll.Question, Poll.Options.Select(o => o.Text)),
            await _channels[3].CreatePollAsync(Poll.Question, Poll.Options.Select(o => o.Text))
        };

        await _context.Polls.AddAsync(Poll);
        await _context.SaveChangesAsync();

        var cachedPollChannels = new List<PollChannel>() {PollChannel.Twitter};
        var cachedChannelPolls = Poll.ChannelPolls.Where(cp => cachedPollChannels.Contains(cp.Channel));

        var cachedVotes = new List<CachedVote>();
        foreach (var channelPoll in cachedChannelPolls)
        {
            var voteCounts = Poll.Options.Select((option, _) => new CachedVote
            {
                Count = 0, 
                Channel = channelPoll.Channel, 
                Option = option, 
                Poll = Poll,
            });
            cachedVotes.AddRange(voteCounts);
        }
        
        await _context.CachedVotes.AddRangeAsync(cachedVotes);
        await _context.SaveChangesAsync();
        
        return RedirectToPage("./Index");
    }
}