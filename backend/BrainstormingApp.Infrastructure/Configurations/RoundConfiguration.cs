using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BrainstormingApp.Core.Entities;

namespace BrainstormingApp.Infrastructure.Configurations;

public class RoundConfiguration : IEntityTypeConfiguration<Round>
{
    public void Configure(EntityTypeBuilder<Round> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RoundNumber)
            .IsRequired();

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>();

        // Unique constraint: Round number must be unique within a session
        builder.HasIndex(r => new { r.SessionId, r.RoundNumber })
            .IsUnique();

        // Indexes
        builder.HasIndex(r => r.SessionId);
        builder.HasIndex(r => r.Status);

        // Relationships
        builder.HasMany(r => r.Ideas)
            .WithOne(i => i.Round)
            .HasForeignKey(i => i.RoundId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
