using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Pages.Polls;

public class Delete : PageModel
{
    private readonly ILogger<Delete> _logger;
    private readonly TallyContext _context;

    public Delete(ILogger<Delete> logger, TallyContext context)
    {
        _logger = logger;
        _context = context;
    }
    
    public Poll Poll { get; set; }
    
    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null) return NotFound();
        Poll = await _context.Polls.Include(p => p.ChannelPolls).SingleAsync(p => p.Id == id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id is null) return NotFound();
        Poll = await _context.Polls.SingleAsync(p => p.Id == id);
        _logger.LogInformation("Deleting poll with ID: {ID}", id);
        return RedirectToPage("./Index");
    }
}