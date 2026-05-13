using Microsoft.EntityFrameworkCore;
using SubscriptionReminder.Api.Models;

namespace SubscriptionReminder.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Subscriptions)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.CustomerId, x.ProviderName, x.SubscriptionNumber }).IsUnique();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasOne(x => x.Subscription)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.PaymentDate).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(x => new { x.SubscriptionId, x.Period, x.Status });
        });
    }
}
