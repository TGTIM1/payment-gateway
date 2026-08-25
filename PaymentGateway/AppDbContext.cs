using Microsoft.EntityFrameworkCore;

namespace PaymentGateway;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Говорим EF Core сохранять PaymentStatus как строку в БД
        modelBuilder.Entity<Payment>()
            .Property(p => p.Status)
            .HasConversion<string>();
    }
}