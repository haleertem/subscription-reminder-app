using SubscriptionReminder.Api.Models;

namespace SubscriptionReminder.Api.DTOs;

public record CreateSubscriptionRequest(
    int CustomerId,
    SubscriptionType Type,
    string ProviderName,
    string SubscriptionNumber,
    SubscriptionStatus Status);

public record UpdateSubscriptionRequest(
    SubscriptionType Type,
    string ProviderName,
    string SubscriptionNumber,
    SubscriptionStatus Status);

public record SubscriptionResponse(
    int Id,
    int CustomerId,
    string CustomerName,
    SubscriptionType Type,
    string ProviderName,
    string SubscriptionNumber,
    SubscriptionStatus Status,
    DateTime CreatedAt);
