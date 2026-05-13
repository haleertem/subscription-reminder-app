using System.ComponentModel.DataAnnotations;

namespace SubscriptionReminder.Api.Models;

public class Subscription
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public SubscriptionType Type { get; set; }

    [Required, MaxLength(120)]
    public string ProviderName { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string SubscriptionNumber { get; set; } = string.Empty;

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
