using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BrainstormingApp.Core.Entities;

namespace BrainstormingApp.Infrastructure.Configurations;

public class SessionLogConfiguration : IEntityTypeConfiguration<SessionLog>
{
    public void Configure(EntityTypeBuilder<SessionLog> builder)
    {
        builder.HasKey(sl => sl.Id);

        builder.Property(sl => sl.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sl => sl.Details)
            .HasMaxLength(2000);

        builder.Property(sl => sl.Timestamp)
            .IsRequired();

        // Indexes
        builder.HasIndex(sl => sl.Timestamp);
        builder.HasIndex(sl => sl.SessionId);
        builder.HasIndex(sl => sl.UserId);
        builder.HasIndex(sl => sl.Action);

        // Relationships
        builder.HasOne(sl => sl.User)
            .WithMany()
            .HasForeignKey(sl => sl.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
