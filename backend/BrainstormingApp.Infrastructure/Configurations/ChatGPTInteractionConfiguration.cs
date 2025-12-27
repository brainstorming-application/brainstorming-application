using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BrainstormingApp.Core.Entities;

namespace BrainstormingApp.Infrastructure.Configurations;

public class ChatGPTInteractionConfiguration : IEntityTypeConfiguration<ChatGPTInteraction>
{
    public void Configure(EntityTypeBuilder<ChatGPTInteraction> builder)
    {
        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.Prompt)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(ci => ci.Response)
            .IsRequired()
            .HasMaxLength(8000);

        builder.Property(ci => ci.InteractionType)
            .HasMaxLength(50);

        builder.Property(ci => ci.TokensUsed)
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(ci => ci.SessionId);
        builder.HasIndex(ci => ci.UserId);
        builder.HasIndex(ci => ci.InteractionType);
        builder.HasIndex(ci => ci.CreatedAt);
    }
}
