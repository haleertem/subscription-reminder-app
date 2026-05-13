using SubscriptionReminder.Api.DTOs;
using SubscriptionReminder.Api.Models;

namespace SubscriptionReminder.Api.Services;

public interface IDebtQueryService
{
    Task<DebtQueryResponse> QueryDebtAsync(Subscription subscription, CancellationToken cancellationToken = default);
}
