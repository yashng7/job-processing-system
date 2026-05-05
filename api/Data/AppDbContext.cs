using JobProcessing.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobProcessing.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Job> Jobs => Set<Job>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(j => j.Id);
            entity.Property(j => j.Name).IsRequired().HasMaxLength(256);
            entity.Property(j => j.Payload).IsRequired();
            entity.Property(j => j.Status).HasConversion<string>();
            entity.Property(j => j.Result).HasMaxLength(4096);
            entity.HasIndex(j => j.Status);
            entity.HasIndex(j => j.CreatedAt);
        });
    }
}