using System.Net.Http.Json;
using SubscriptionReminder.Api.DTOs;

namespace SubscriptionReminder.Api.Services;

public class PaymentGatewayService : IPaymentGatewayService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PaymentGatewayService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<PaymentGatewayResponse> PayAsync(PaymentGatewayRequest request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("MockApi");
        var response = await client.PostAsJsonAsync("payments", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PaymentGatewayResponse>(cancellationToken: cancellationToken);
        return result ?? new PaymentGatewayResponse(false, string.Empty, "Mock ödeme servisi cevap döndürmedi.");
    }
}
