using Microsoft.EntityFrameworkCore;
using Victorina.Domain.Entities;

namespace Victorina.Infrastructure.Data;

public class VictorinaDbContext : DbContext
{
    public VictorinaDbContext(DbContextOptions<VictorinaDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<GamePlayer> GamePlayers => Set<GamePlayer>();
    public DbSet<GameQuestion> GameQuestions => Set<GameQuestion>();
    public DbSet<GameAnswer> GameAnswers => Set<GameAnswer>();
    public DbSet<GameSettings> GameSettings => Set<GameSettings>();
    public DbSet<UserQuestionHistory> UserQuestionHistories => Set<UserQuestionHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.TelegramId).IsUnique();
            entity.HasIndex(e => e.Username);
            entity.HasIndex(e => e.Phone);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        // Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Emoji).HasMaxLength(10);
        });

        // Question
        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Questions)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Friendship
        modelBuilder.Entity<Friendship>(entity =>
        {
            entity.HasOne(e => e.Requester)
                .WithMany(u => u.FriendshipsInitiated)
                .HasForeignKey(e => e.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Addressee)
                .WithMany(u => u.FriendshipsReceived)
                .HasForeignKey(e => e.AddresseeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.RequesterId, e.AddresseeId }).IsUnique();
        });

        // Game
        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasIndex(e => e.GameGuid).IsUnique();
            entity.HasIndex(e => e.Status);

            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Winner)
                .WithMany()
                .HasForeignKey(e => e.WinnerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // GamePlayer
        modelBuilder.Entity<GamePlayer>(entity =>
        {
            entity.HasOne(e => e.Game)
                .WithMany(g => g.Players)
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(u => u.GamePlayers)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.GameId, e.UserId }).IsUnique();
        });

        // GameQuestion
        modelBuilder.Entity<GameQuestion>(entity =>
        {
            entity.HasOne(e => e.Game)
                .WithMany(g => g.Questions)
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Question)
                .WithMany()
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.GameId, e.OrderIndex }).IsUnique();
        });

        // GameAnswer
        modelBuilder.Entity<GameAnswer>(entity =>
        {
            entity.HasOne(e => e.GamePlayer)
                .WithMany(gp => gp.Answers)
                .HasForeignKey(e => e.GamePlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.GameQuestion)
                .WithMany()
                .HasForeignKey(e => e.GameQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.GamePlayerId, e.GameQuestionId }).IsUnique();
        });

        // GameSettings
        modelBuilder.Entity<GameSettings>(entity =>
        {
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Key).HasMaxLength(100);
        });

        // UserQuestionHistory
        modelBuilder.Entity<UserQuestionHistory>(entity =>
        {
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.QuestionTranslationGroupId });
            entity.HasIndex(e => e.ShownAt);
        });

        // Seed default settings
        modelBuilder.Entity<GameSettings>().HasData(
            new GameSettings { Id = 1, Key = "QuestionTimeSeconds", Value = "15", Description = "Время на ответ (секунды)" },
            new GameSettings { Id = 2, Key = "QuestionsPerGame", Value = "10", Description = "Количество вопросов в игре" }
        );
    }
}
