using JobProcessing.Worker.Models;
using Microsoft.EntityFrameworkCore;

namespace JobProcessing.Worker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Job> Jobs => Set<Job>();

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
        });
    }
}