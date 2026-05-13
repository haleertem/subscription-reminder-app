using Microsoft.EntityFrameworkCore;
using SubscriptionReminder.Api.Data;
using SubscriptionReminder.Api.DTOs;
using SubscriptionReminder.Api.Models;

namespace SubscriptionReminder.Api.Services;

public class ReminderService : IReminderService
{
    private readonly AppDbContext _db;
    private readonly IDebtQueryService _debtQueryService;
    private readonly INotificationService _notificationService;

    public ReminderService(AppDbContext db, IDebtQueryService debtQueryService, INotificationService notificationService)
    {
        _db = db;
        _debtQueryService = debtQueryService;
        _notificationService = notificationService;
    }

    public async Task<IReadOnlyList<ReminderResponse>> CheckCustomerRemindersAsync(int customerId, int daysBeforeDueDate, CancellationToken cancellationToken = default)
    {
        var customer = await _db.Customers
            .Include(c => c.Subscriptions.Where(s => s.Status == SubscriptionStatus.Active))
            .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);

        if (customer is null) return Array.Empty<ReminderResponse>();

        var today = DateTime.UtcNow.Date;
        var latestReminderDate = today.AddDays(daysBeforeDueDate);
        var result = new List<ReminderResponse>();

        foreach (var subscription in customer.Subscriptions)
        {
            var debt = await _debtQueryService.QueryDebtAsync(subscription, cancellationToken);
            var alreadyPaid = await _db.Payments.AnyAsync(p =>
                p.SubscriptionId == subscription.Id &&
                p.Period == debt.Period &&
                p.Status == PaymentStatus.Successful,
                cancellationToken);

            var shouldSend = debt.DueDate.Date >= today && debt.DueDate.Date <= latestReminderDate && !alreadyPaid;
            var sent = false;
            var message = alreadyPaid
                ? "Bu dönem için başarılı ödeme bulundu, hatırlatma gönderilmedi."
                : "Son ödeme tarihi henüz hatırlatma aralığında değil.";

            if (shouldSend)
            {
                var text = $"{subscription.ProviderName} aboneliğiniz için {debt.Period} döneminde {debt.Amount:N2} TL ödeme son tarihi {debt.DueDate:yyyy-MM-dd}.";
                var notification = await _notificationService.SendAsync(new NotificationRequest(
                    customer.Id,
                    subscription.Id,
                    string.IsNullOrWhiteSpace(customer.PhoneNumber) ? NotificationChannel.Email : NotificationChannel.Sms,
                    string.IsNullOrWhiteSpace(customer.PhoneNumber) ? customer.Email : customer.PhoneNumber!,
                    text), cancellationToken);

                sent = notification.Sent;
                message = sent ? "Hatırlatma gönderildi." : "Hatırlatma gönderilemedi.";
            }

            result.Add(new ReminderResponse(subscription.Id, subscription.ProviderName, subscription.SubscriptionNumber,
                debt.Period, debt.DueDate, debt.Amount, alreadyPaid, sent, message));
        }

        return result;
    }
}
