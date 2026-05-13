using SubscriptionReminder.Api.Models;

namespace SubscriptionReminder.Api.DTOs;

public record CreatePaymentRequest(int SubscriptionId);
public record PaymentGatewayRequest(int SubscriptionId, decimal Amount, string Period);
public record PaymentGatewayResponse(bool IsSuccessful, string TransactionReference, string? ErrorMessage);

public record PaymentResponse(
    int Id,
    int SubscriptionId,
    string ProviderName,
    string SubscriptionNumber,
    decimal Amount,
    DateTime PaymentDate,
    string Period,
    PaymentStatus Status,
    string? TransactionReference,
    string? FailureReason);
