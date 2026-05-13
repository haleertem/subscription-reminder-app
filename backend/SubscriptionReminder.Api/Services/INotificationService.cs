using SubscriptionReminder.Api.DTOs;

namespace SubscriptionReminder.Api.Services;

public interface INotificationService
{
    Task<NotificationResponse> SendAsync(NotificationRequest request, CancellationToken cancellationToken = default);
}
