using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Web.Models;

namespace Web.Data;

public class TallyContext : IdentityDbContext<User>
{
    public DbSet<Poll> Polls => Set<Poll>();
    public DbSet<ChannelPoll> ChannelPolls => Set<ChannelPoll>();
    public DbSet<CachedVote> CachedVotes => Set<CachedVote>();
    public DbSet<LiveVote> LiveVotes => Set<LiveVote>();
    public DbSet<Option> Options => Set<Option>();
    public TallyContext(DbContextOptions<TallyContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<CachedVote>().HasIndex(v => v.Channel);
        builder.Entity<CachedVote>()
            .HasIndex(v => new { v.Channel, v.OptionId })
            .IsUnique();
        
        builder.Entity<ChannelPoll>().HasIndex(p => p.Channel);
        builder.Entity<ChannelPoll>().HasIndex(p => p.PrimaryIdentifier);
        builder.Entity<ChannelPoll>().HasIndex(p => p.AuxiliaryIdentifier);
        builder.Entity<ChannelPoll>()
            .HasIndex(p => new { p.Channel, p.PrimaryIdentifier })
            .IsUnique();
        builder.Entity<ChannelPoll>()
            .HasIndex(p => new { p.Channel, p.AuxiliaryIdentifier })
            .IsUnique();

        builder.Entity<LiveVote>().HasIndex(v => v.Channel);
        builder.Entity<LiveVote>().HasIndex(v => v.UserIdentifier);
        builder.Entity<LiveVote>()
            .HasIndex(v => new { v.Channel, v.UserIdentifier })
            .IsUnique();
    }
}