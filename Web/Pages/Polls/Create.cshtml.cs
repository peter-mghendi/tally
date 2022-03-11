using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Web.Channels;
using Web.Data;
using Web.Models;

namespace Web.Pages.Polls;

[Authorize]
public class Create : PageModel
{
    private readonly TallyContext _context;
    private readonly TelegramChannel _channel;
    private readonly ILogger<Create> _logger;
    private readonly UserManager<User> _userManager;

    [BindProperty] public Poll Poll { get; set; }

    public Create(TallyContext context, TelegramChannel channel, ILogger<Create> logger, UserManager<User> userManager)
    {
        _context = context;
        _channel = channel;
        _logger = logger;
        _userManager = userManager;
    }

    public IActionResult OnGet()
    {
        Poll = new();
        Poll.Options = Enumerable.Repeat(new Option(), 4).ToList();
        return Page();
    }

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public async Task<IActionResult> OnPostAsync()
    {
        Poll.Creator = await _userManager.GetUserAsync(User);

        Poll.ChannelPolls = new List<ChannelPoll>
        {
            await _channel.CreatePollAsync(Poll.Question, Poll.Options.Select(o => o.Text))
        };

        await _context.Polls.AddAsync(Poll);

        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}