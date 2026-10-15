using Microsoft.EntityFrameworkCore;
using PaymentAgent.Domain;

namespace PaymentAgent.Data;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options):base(options)
    {}

    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentStatusEvent> StatusEvents => Set<PaymentStatusEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.OrderId);
            entity.Property(p => p.Status).HasConversion<string>();
        });
        modelBuilder.Entity<PaymentStatusEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PaymentId);
            entity.Property(e => e.Status).HasConversion<string>();
        });
    }
}