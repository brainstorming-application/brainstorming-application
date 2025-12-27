using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BrainstormingApp.Core.Entities;

namespace BrainstormingApp.Infrastructure.Configurations;

public class BrainstormingSessionConfiguration : IEntityTypeConfiguration<BrainstormingSession>
{
    public void Configure(EntityTypeBuilder<BrainstormingSession> builder)
    {
        builder.HasKey(bs => bs.Id);

        builder.Property(bs => bs.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(bs => bs.CurrentRound)
            .HasDefaultValue(0);

        builder.Property(bs => bs.TotalRounds)
            .HasDefaultValue(5); // For 6-3-5 method

        builder.Property(bs => bs.RoundDurationMinutes)
            .HasDefaultValue(5);

        // Indexes
        builder.HasIndex(bs => bs.Status);
        builder.HasIndex(bs => bs.TeamId);
        builder.HasIndex(bs => bs.TopicId);

        // Relationships
        builder.HasMany(bs => bs.Rounds)
            .WithOne(r => r.Session)
            .HasForeignKey(r => r.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(bs => bs.Ideas)
            .WithOne(i => i.Session)
            .HasForeignKey(i => i.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(bs => bs.SessionLogs)
            .WithOne(sl => sl.Session)
            .HasForeignKey(sl => sl.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(bs => bs.ChatGPTInteractions)
            .WithOne(ci => ci.Session)
            .HasForeignKey(ci => ci.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
