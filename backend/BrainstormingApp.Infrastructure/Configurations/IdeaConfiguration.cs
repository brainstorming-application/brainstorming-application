using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BrainstormingApp.Core.Entities;

namespace BrainstormingApp.Infrastructure.Configurations;

public class IdeaConfiguration : IEntityTypeConfiguration<Idea>
{
    public void Configure(EntityTypeBuilder<Idea> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(i => i.OrderInRound)
            .IsRequired();

        builder.Property(i => i.IsAIGenerated)
            .HasDefaultValue(false);

        builder.Property(i => i.AIAnnotation)
            .HasMaxLength(1000);

        builder.Property(i => i.SubmittedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(i => i.SubmittedAt);
        builder.HasIndex(i => i.RoundId);
        builder.HasIndex(i => i.SessionId);
        builder.HasIndex(i => i.UserId);
        builder.HasIndex(i => i.IsAIGenerated);
    }
}
