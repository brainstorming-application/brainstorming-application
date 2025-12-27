using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BrainstormingApp.Core.Entities;

namespace BrainstormingApp.Infrastructure.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.StartDate)
            .IsRequired();

        builder.Property(e => e.EndDate)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>();

        // Indexes
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.StartDate);

        // Relationships
        builder.HasOne(e => e.CreatedBy)
            .WithMany()
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Topics)
            .WithOne(t => t.Event)
            .HasForeignKey(t => t.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.EventParticipants)
            .WithOne(ep => ep.Event)
            .HasForeignKey(ep => ep.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Teams)
            .WithOne(t => t.Event)
            .HasForeignKey(t => t.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
