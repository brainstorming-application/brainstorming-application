using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BrainstormingApp.Core.Entities;

namespace BrainstormingApp.Infrastructure.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.MaxMembers)
            .HasDefaultValue(6); // For 6-3-5 method

        // Indexes
        builder.HasIndex(t => t.EventId);
        builder.HasIndex(t => t.LeaderId);

        // Relationships
        builder.HasMany(t => t.TeamMembers)
            .WithOne(tm => tm.Team)
            .HasForeignKey(tm => tm.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.BrainstormingSessions)
            .WithOne(bs => bs.Team)
            .HasForeignKey(bs => bs.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
