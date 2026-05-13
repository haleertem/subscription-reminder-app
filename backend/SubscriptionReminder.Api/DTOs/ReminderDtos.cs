using SubscriptionReminder.Api.Models;

namespace SubscriptionReminder.Api.DTOs;

public record NotificationRequest(int CustomerId, int SubscriptionId, NotificationChannel Channel, string Recipient, string Message);
public record NotificationResponse(bool Sent, string Channel, string Recipient, string Message);

public record ReminderResponse(
    int SubscriptionId,
    string ProviderName,
    string SubscriptionNumber,
    string Period,
    DateTime DueDate,
    decimal Amount,
    bool AlreadyPaid,
    bool ReminderSent,
    string Message);
