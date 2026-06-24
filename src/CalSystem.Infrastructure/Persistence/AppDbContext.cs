using CalSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CalSystem.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<ServiceOrder> ServiceOrders => Set<ServiceOrder>();
    public DbSet<Technician> Technicians => Set<Technician>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServiceOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Equipment).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ProblemDescription).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Status).HasConversion<string>();
        });

        modelBuilder.Entity<Technician>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
        });
    }
}
