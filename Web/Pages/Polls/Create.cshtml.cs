using LinqToTwitter;
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
    private readonly IChannel _telegramChannel;
    private readonly IChannel _twitterChannel;
    private readonly UserManager<User> _userManager;

    [BindProperty] public Poll Poll { get; set; }

    public Create(
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
            await _telegramChannel.CreatePollAsync(Poll.Question, Poll.Options.Select(o => o.Text)),
            await _twitterChannel.CreatePollAsync(Poll.Question, Poll.Options.Select(o => o.Text))
        };

        await _context.Polls.AddAsync(Poll);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}