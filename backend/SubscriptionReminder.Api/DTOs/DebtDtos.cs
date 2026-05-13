using SubscriptionReminder.Api.Models;

namespace SubscriptionReminder.Api.DTOs;

public record DebtQueryResponse(
    int SubscriptionId,
    SubscriptionType Type,
    string ProviderName,
    string SubscriptionNumber,
    decimal Amount,
    DateTime DueDate,
    string Period,
    bool IsPaidForPeriod);
