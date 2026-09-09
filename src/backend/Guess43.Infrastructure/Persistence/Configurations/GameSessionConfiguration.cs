using Guess43.Domain.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Guess43.Infrastructure.Persistence.Configurations;

internal sealed class GameSessionConfiguration : IEntityTypeConfiguration<GameSession>
{
    public void Configure(EntityTypeBuilder<GameSession> builder)
    {
        builder.ToTable("game_sessions");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.UserId).IsRequired();
        builder.Property(g => g.TargetNumber).IsRequired();
        builder.Property(g => g.GuessCount).IsRequired();
        builder.Property(g => g.Status).IsRequired().HasConversion<int>();
        builder.Property(g => g.StartedAtUtc).IsRequired();
        builder.Property(g => g.CompletedAtUtc);

        // Map the domain concurrency token to PostgreSQL's system xmin column.
        builder.Property(g => g.Version)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        builder.HasOne<Domain.Users.User>()
            .WithMany()
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(g => g.UserId).HasDatabaseName("ix_game_sessions_user_id");

        // At most one active game per user, enforced by a partial unique index.
        // GameStatus.Active == 1.
        builder.HasIndex(g => g.UserId)
            .IsUnique()
            .HasFilter("\"Status\" = 1")
            .HasDatabaseName("ux_game_sessions_active_per_user");
    }
}
