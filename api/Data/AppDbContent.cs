using JobProcessing.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobProcessing.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(j => j.Id);

            entity.Property(j => j.Id)
                  .HasColumnType("uuid")
                  .ValueGeneratedNever();

            entity.Property(j => j.Name)
                  .IsRequired()
                  .HasMaxLength(256);

            entity.Property(j => j.Payload)
                  .IsRequired();

            entity.Property(j => j.Status)
                  .IsRequired()
                  .HasConversion<string>();

            entity.Property(j => j.Result)
                  .HasMaxLength(4096);

            entity.Property(j => j.RetryCount)
                  .HasDefaultValue(0);

            entity.Property(j => j.CreatedAt)
                  .HasColumnType("timestamp with time zone");

            entity.Property(j => j.UpdatedAt)
                  .HasColumnType("timestamp with time zone");

            entity.HasIndex(j => j.Status);
            entity.HasIndex(j => j.CreatedAt);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Id)
                  .HasColumnType("uuid")
                  .ValueGeneratedNever();

            entity.Property(u => u.Username)
                  .IsRequired()
                  .HasMaxLength(64);

            entity.Property(u => u.PasswordHash)
                  .IsRequired();

            entity.Property(u => u.Role)
                  .IsRequired()
                  .HasMaxLength(32);

            entity.Property(u => u.CreatedAt)
                  .HasColumnType("timestamp with time zone");

            entity.HasIndex(u => u.Username)
                  .IsUnique();

            entity.HasMany(u => u.RefreshTokens)
                  .WithOne(r => r.User)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.Id)
                  .HasColumnType("uuid")
                  .ValueGeneratedNever();

            entity.Property(r => r.Token)
                  .IsRequired()
                  .HasMaxLength(512);

            entity.Property(r => r.ExpiresAt)
                  .HasColumnType("timestamp with time zone");

            entity.Property(r => r.CreatedAt)
                  .HasColumnType("timestamp with time zone");

            entity.Property(r => r.CreatedByIp)
                  .HasMaxLength(64);

            entity.Property(r => r.RevokedByIp)
                  .HasMaxLength(64);

            entity.Property(r => r.ReplacedByToken)
                  .HasMaxLength(512);

            entity.HasIndex(r => r.Token);
        });
    }
}