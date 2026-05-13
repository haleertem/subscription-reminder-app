using SubscriptionReminder.Api.DTOs;

namespace SubscriptionReminder.Api.Services;

public interface IReminderService
{
    Task<IReadOnlyList<ReminderResponse>> CheckCustomerRemindersAsync(int customerId, int daysBeforeDueDate, CancellationToken cancellationToken = default);
}
