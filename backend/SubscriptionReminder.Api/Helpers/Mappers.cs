using SubscriptionReminder.Api.DTOs;
using SubscriptionReminder.Api.Models;

namespace SubscriptionReminder.Api.Helpers;

public static class Mappers
{
    public static CustomerResponse ToResponse(this Customer c) =>
        new(c.Id, c.FullName, c.Email, c.PhoneNumber, c.CreatedAt);

    public static SubscriptionResponse ToResponse(this Subscription s) =>
        new(s.Id, s.CustomerId, s.Customer?.FullName ?? string.Empty, s.Type, s.ProviderName,
            s.SubscriptionNumber, s.Status, s.CreatedAt);

    public static PaymentResponse ToResponse(this Payment p) =>
        new(p.Id, p.SubscriptionId, p.Subscription?.ProviderName ?? string.Empty,
            p.Subscription?.SubscriptionNumber ?? string.Empty, p.Amount, p.PaymentDate, p.Period,
            p.Status, p.TransactionReference, p.FailureReason);
}
