using Microsoft.EntityFrameworkCore;
using PaymentGateway.Models;

namespace PaymentGateway;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Payment>()
            .Property(p => p.Status)
            .HasConversion<string>();
        modelBuilder.Entity<Payment>()
            .HasIndex(i => i.IdempotencyKey)
            .IsUnique();
    }
}