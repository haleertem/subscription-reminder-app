using System.Net.Http.Json;
using SubscriptionReminder.Api.DTOs;

namespace SubscriptionReminder.Api.Services;

public class NotificationService : INotificationService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public NotificationService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<NotificationResponse> SendAsync(NotificationRequest request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("MockApi");
        var response = await client.PostAsJsonAsync("notifications", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<NotificationResponse>(cancellationToken: cancellationToken);
        return result ?? new NotificationResponse(false, request.Channel.ToString(), request.Recipient, request.Message);
    }
}
