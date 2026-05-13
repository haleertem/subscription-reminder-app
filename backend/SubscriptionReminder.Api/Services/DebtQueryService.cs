using System.Net.Http.Json;
using SubscriptionReminder.Api.DTOs;
using SubscriptionReminder.Api.Models;

namespace SubscriptionReminder.Api.Services;

public class DebtQueryService : IDebtQueryService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DebtQueryService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<DebtQueryResponse> QueryDebtAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("MockApi");
        var url = $"debts?subscriptionId={subscription.Id}&type={subscription.Type}&providerName={Uri.EscapeDataString(subscription.ProviderName)}&subscriptionNumber={Uri.EscapeDataString(subscription.SubscriptionNumber)}";
        var result = await client.GetFromJsonAsync<DebtQueryResponse>(url, cancellationToken);
        return result ?? throw new InvalidOperationException("Mock borç servisi geçerli bir cevap döndürmedi.");
    }
}
