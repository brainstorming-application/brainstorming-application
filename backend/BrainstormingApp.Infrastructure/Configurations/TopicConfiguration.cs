using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BrainstormingApp.Core.Entities;

namespace BrainstormingApp.Infrastructure.Configurations;

public class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(t => t.Description)
            .HasMaxLength(2000);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>();

        // Indexes
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.EventId);

        // Relationships
        builder.HasMany(t => t.BrainstormingSessions)
            .WithOne(bs => bs.Topic)
            .HasForeignKey(bs => bs.TopicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
