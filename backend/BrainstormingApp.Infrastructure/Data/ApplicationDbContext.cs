using Microsoft.EntityFrameworkCore;
using BrainstormingApp.Core.Entities;

namespace BrainstormingApp.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<EventParticipant> EventParticipants => Set<EventParticipant>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<BrainstormingSession> BrainstormingSessions => Set<BrainstormingSession>();
    public DbSet<Round> Rounds => Set<Round>();
    public DbSet<Idea> Ideas => Set<Idea>();
    public DbSet<ChatGPTInteraction> ChatGPTInteractions => Set<ChatGPTInteraction>();
    public DbSet<SessionLog> SessionLogs => Set<SessionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure enums to be stored as strings
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        modelBuilder.Entity<Event>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Topic>()
            .Property(t => t.Status)
            .HasConversion<string>();

        modelBuilder.Entity<EventParticipant>()
            .Property(ep => ep.Role)
            .HasConversion<string>();

        modelBuilder.Entity<BrainstormingSession>()
            .Property(bs => bs.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Round>()
            .Property(r => r.Status)
            .HasConversion<string>();

        // Configure relationships

        // User relationships
        modelBuilder.Entity<User>()
            .HasMany(u => u.EventParticipants)
            .WithOne(ep => ep.User)
            .HasForeignKey(ep => ep.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.TeamMemberships)
            .WithOne(tm => tm.User)
            .HasForeignKey(tm => tm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.LeadingTeams)
            .WithOne(t => t.Leader)
            .HasForeignKey(t => t.LeaderId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Ideas)
            .WithOne(i => i.User)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Event relationships
        modelBuilder.Entity<Event>()
            .HasOne(e => e.CreatedBy)
            .WithMany()
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Event>()
            .HasMany(e => e.Topics)
            .WithOne(t => t.Event)
            .HasForeignKey(t => t.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Event>()
            .HasMany(e => e.EventParticipants)
            .WithOne(ep => ep.Event)
            .HasForeignKey(ep => ep.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Event>()
            .HasMany(e => e.Teams)
            .WithOne(t => t.Event)
            .HasForeignKey(t => t.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // Topic relationships
        modelBuilder.Entity<Topic>()
            .HasMany(t => t.BrainstormingSessions)
            .WithOne(bs => bs.Topic)
            .HasForeignKey(bs => bs.TopicId)
            .OnDelete(DeleteBehavior.Cascade);

        // Team relationships
        modelBuilder.Entity<Team>()
            .HasMany(t => t.TeamMembers)
            .WithOne(tm => tm.Team)
            .HasForeignKey(tm => tm.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Team>()
            .HasMany(t => t.BrainstormingSessions)
            .WithOne(bs => bs.Team)
            .HasForeignKey(bs => bs.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        // BrainstormingSession relationships
        modelBuilder.Entity<BrainstormingSession>()
            .HasMany(bs => bs.Rounds)
            .WithOne(r => r.Session)
            .HasForeignKey(r => r.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BrainstormingSession>()
            .HasMany(bs => bs.Ideas)
            .WithOne(i => i.Session)
            .HasForeignKey(i => i.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BrainstormingSession>()
            .HasMany(bs => bs.SessionLogs)
            .WithOne(sl => sl.Session)
            .HasForeignKey(sl => sl.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BrainstormingSession>()
            .HasMany(bs => bs.ChatGPTInteractions)
            .WithOne(ci => ci.Session)
            .HasForeignKey(ci => ci.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Round relationships
        modelBuilder.Entity<Round>()
            .HasMany(r => r.Ideas)
            .WithOne(i => i.Round)
            .HasForeignKey(i => i.RoundId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraints
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<EventParticipant>()
            .HasIndex(ep => new { ep.EventId, ep.UserId })
            .IsUnique();

        modelBuilder.Entity<TeamMember>()
            .HasIndex(tm => new { tm.TeamId, tm.UserId })
            .IsUnique();

        modelBuilder.Entity<Round>()
            .HasIndex(r => new { r.SessionId, r.RoundNumber })
            .IsUnique();

        // Indexes for performance
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Role);

        modelBuilder.Entity<Event>()
            .HasIndex(e => e.Status);

        modelBuilder.Entity<BrainstormingSession>()
            .HasIndex(bs => bs.Status);

        modelBuilder.Entity<Idea>()
            .HasIndex(i => i.SubmittedAt);

        modelBuilder.Entity<SessionLog>()
            .HasIndex(sl => sl.Timestamp);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
