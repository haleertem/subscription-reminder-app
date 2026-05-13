using System.ComponentModel.DataAnnotations;

namespace SubscriptionReminder.Api.Models;

public class Customer
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(160)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
