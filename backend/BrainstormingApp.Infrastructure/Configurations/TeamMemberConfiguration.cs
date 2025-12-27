using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BrainstormingApp.Core.Entities;

namespace BrainstormingApp.Infrastructure.Configurations;

public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.HasKey(tm => tm.Id);

        builder.Property(tm => tm.JoinedAt)
            .IsRequired();

        // Unique constraint: User can be member of a team only once
        builder.HasIndex(tm => new { tm.TeamId, tm.UserId })
            .IsUnique();

        // Indexes
        builder.HasIndex(tm => tm.TeamId);
        builder.HasIndex(tm => tm.UserId);
    }
}
