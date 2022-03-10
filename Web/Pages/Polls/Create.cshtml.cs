using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
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

    public async Task<IActionResult> OnGetAsync()
    {
        Poll = new ();
        return Page();
    }

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public async Task<IActionResult> OnPostAsync()
    {
        _logger.LogInformation($"{Poll.Creator is null}");
        Poll.Creator = await _userManager.GetUserAsync(User);
        _logger.LogInformation($"{Poll.Creator is null}");

        _logger.LogInformation("Valid.");
        // var options = new List<string> {"Option 1", "Option 2", "Option 3", "Option 4"};
        var telegramChannelPoll = await _channel.CreatePollAsync(Poll.Question, Poll.Options.Select(o => o.Text));
        
        Poll.ChannelPolls = new List<ChannelPoll> {telegramChannelPoll};
        await _context.Polls.AddAsync(Poll);
        
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}