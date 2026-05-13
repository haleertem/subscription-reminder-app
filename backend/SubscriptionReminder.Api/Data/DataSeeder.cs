using SubscriptionReminder.Api.Models;

namespace SubscriptionReminder.Api.Data;

public static class DataSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Customers.Any()) return;

        var customer = new Customer
        {
            FullName = "Demo Customer",
            Email = "demo@example.com",
            PhoneNumber = "+905551112233",
            Subscriptions = new List<Subscription>
            {
                new()
                {
                    Type = SubscriptionType.Electricity,
                    ProviderName = "EnerjiSA Mock",
                    SubscriptionNumber = "ELK-100001",
                    Status = SubscriptionStatus.Active
                },
                new()
                {
                    Type = SubscriptionType.Internet,
                    ProviderName = "FiberNet Mock",
                    SubscriptionNumber = "INT-200002",
                    Status = SubscriptionStatus.Active
                }
            }
        };

        db.Customers.Add(customer);
        db.SaveChanges();
    }
}
