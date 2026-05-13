namespace SubscriptionReminder.Api.DTOs;

public record CustomerSummaryResponse(
    int CustomerId,
    string CustomerName,
    int ActiveSubscriptionCount,
    int UnpaidCurrentMonthSubscriptionCount,
    decimal SuccessfulPaymentTotal,
    IEnumerable<SubscriptionResponse> ActiveSubscriptions,
    IEnumerable<PaymentResponse> RecentPayments);
