using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace Shared.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<SensorData> SensorData => Set<SensorData>();
    public DbSet<SensorError> SensorErrors => Set<SensorError>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SensorData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SensorId).IsRequired();
            entity.Property(e => e.SensorType).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired();
        });

        modelBuilder.Entity<SensorError>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SensorId).IsRequired();
            entity.Property(e => e.SensorType).IsRequired();
            entity.Property(e => e.ErrorTimestamp).IsRequired();
            entity.Property(e => e.ErrorMessage).IsRequired();
        });
    }
}
