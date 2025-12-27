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

        // Apply all configurations from assembly (Configurations folder)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
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
