using Microsoft.AspNetCore.Identity;

namespace Web.Models;

public class User : IdentityUser
{
    public ICollection<Poll> Polls { get; set; } = new List<Poll>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}