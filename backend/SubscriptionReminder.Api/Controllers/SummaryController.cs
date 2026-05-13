using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubscriptionReminder.Api.Data;
using SubscriptionReminder.Api.DTOs;
using SubscriptionReminder.Api.Helpers;
using SubscriptionReminder.Api.Models;

namespace SubscriptionReminder.Api.Controllers;

[ApiController]
[Route("api/customers/{customerId:int}/summary")]
public class SummaryController : ControllerBase
{
    private readonly AppDbContext _db;

    public SummaryController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<CustomerSummaryResponse>> Get(int customerId, CancellationToken cancellationToken)
    {
        var customer = await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);
        if (customer is null) return NotFound();

        var currentPeriod = DateTime.UtcNow.ToString("yyyy-MM");
        var activeSubscriptions = await _db.Subscriptions
            .Include(s => s.Customer)
            .AsNoTracking()
            .Where(s => s.CustomerId == customerId && s.Status == SubscriptionStatus.Active)
            .ToListAsync(cancellationToken);

        var paidSubscriptionIds = await _db.Payments
            .AsNoTracking()
            .Where(p => p.Period == currentPeriod && p.Status == PaymentStatus.Successful)
            .Select(p => p.SubscriptionId)
            .ToListAsync(cancellationToken);

        var recentPayments = await _db.Payments
            .Include(p => p.Subscription)
            .AsNoTracking()
            .Where(p => p.Subscription!.CustomerId == customerId)
            .OrderByDescending(p => p.PaymentDate)
            .Take(10)
            .ToListAsync(cancellationToken);

        var total = await _db.Payments
            .AsNoTracking()
            .Where(p => p.Subscription!.CustomerId == customerId && p.Status == PaymentStatus.Successful)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;

        return Ok(new CustomerSummaryResponse(
            customer.Id,
            customer.FullName,
            activeSubscriptions.Count,
            activeSubscriptions.Count(s => !paidSubscriptionIds.Contains(s.Id)),
            total,
            activeSubscriptions.Select(s => s.ToResponse()),
            recentPayments.Select(p => p.ToResponse())));
    }
}
