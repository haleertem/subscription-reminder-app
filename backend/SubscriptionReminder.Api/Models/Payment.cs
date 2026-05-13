using System.ComponentModel.DataAnnotations;

namespace SubscriptionReminder.Api.Models;

public class Payment
{
    public int Id { get; set; }

    public int SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }

    [Range(0.01, 999999)]
    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(7)]
    public string Period { get; set; } = string.Empty; // yyyy-MM

    public PaymentStatus Status { get; set; }

    [MaxLength(120)]
    public string? TransactionReference { get; set; }

    [MaxLength(300)]
    public string? FailureReason { get; set; }
}
