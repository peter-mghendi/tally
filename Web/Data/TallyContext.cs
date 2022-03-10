using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Web.Models;

namespace Web.Data;

public class TallyContext : IdentityDbContext<User>
{
    public DbSet<Poll> Polls => Set<Poll>();
    public DbSet<ChannelPoll> ChannelPolls => Set<ChannelPoll>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<Option> Options => Set<Option>();
    public TallyContext(DbContextOptions<TallyContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ChannelPoll>()
            .HasIndex(p => new { p.Channel, p.Identifier })
            .IsUnique();
    }
}