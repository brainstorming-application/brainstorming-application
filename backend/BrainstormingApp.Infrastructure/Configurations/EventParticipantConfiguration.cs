using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BrainstormingApp.Core.Entities;

namespace BrainstormingApp.Infrastructure.Configurations;

public class EventParticipantConfiguration : IEntityTypeConfiguration<EventParticipant>
{
    public void Configure(EntityTypeBuilder<EventParticipant> builder)
    {
        builder.HasKey(ep => ep.Id);

        builder.Property(ep => ep.Role)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(ep => ep.JoinedAt)
            .IsRequired();

        // Unique constraint: User can join an event only once
        builder.HasIndex(ep => new { ep.EventId, ep.UserId })
            .IsUnique();

        // Indexes
        builder.HasIndex(ep => ep.EventId);
        builder.HasIndex(ep => ep.UserId);
    }
}
