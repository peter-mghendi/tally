using Microsoft.AspNetCore.Identity;

namespace Web.Models;

public class User : IdentityUser
{
    public IEnumerable<Poll> Polls { get; set; } = new List<Poll>();
    public IEnumerable<Vote> Votes { get; set; } = new List<Vote>();
}