using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubscriptionReminder.Api.Data;
using SubscriptionReminder.Api.DTOs;
using SubscriptionReminder.Api.Helpers;
using SubscriptionReminder.Api.Models;
using SubscriptionReminder.Api.Services;

namespace SubscriptionReminder.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IDebtQueryService _debtQueryService;

    public SubscriptionsController(AppDbContext db, IDebtQueryService debtQueryService)
    {
        _db = db;
        _debtQueryService = debtQueryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SubscriptionResponse>>> GetAll([FromQuery] int? customerId, CancellationToken cancellationToken)
    {
        var query = _db.Subscriptions.Include(s => s.Customer).AsNoTracking();
        if (customerId.HasValue) query = query.Where(s => s.CustomerId == customerId.Value);

        var subscriptions = await query
            .OrderBy(s => s.ProviderName)
            .Select(s => s.ToResponse())
            .ToListAsync(cancellationToken);

        return Ok(subscriptions);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SubscriptionResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var subscription = await _db.Subscriptions
            .Include(s => s.Customer)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        return subscription is null ? NotFound() : Ok(subscription.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<SubscriptionResponse>> Create(CreateSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var customerExists = await _db.Customers.AnyAsync(c => c.Id == request.CustomerId, cancellationToken);
        if (!customerExists) return BadRequest("Müşteri bulunamadı.");

        var duplicate = await _db.Subscriptions.AnyAsync(s =>
            s.CustomerId == request.CustomerId &&
            s.ProviderName == request.ProviderName &&
            s.SubscriptionNumber == request.SubscriptionNumber,
            cancellationToken);

        if (duplicate) return Conflict("Bu müşteri için aynı sağlayıcı ve abonelik numarası zaten kayıtlı.");

        var subscription = new Subscription
        {
            CustomerId = request.CustomerId,
            Type = request.Type,
            ProviderName = request.ProviderName.Trim(),
            SubscriptionNumber = request.SubscriptionNumber.Trim(),
            Status = request.Status
        };

        _db.Subscriptions.Add(subscription);
        await _db.SaveChangesAsync(cancellationToken);

        await _db.Entry(subscription).Reference(s => s.Customer).LoadAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = subscription.Id }, subscription.ToResponse());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<SubscriptionResponse>> Update(int id, UpdateSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var subscription = await _db.Subscriptions.Include(s => s.Customer).FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (subscription is null) return NotFound();

        subscription.Type = request.Type;
        subscription.ProviderName = request.ProviderName.Trim();
        subscription.SubscriptionNumber = request.SubscriptionNumber.Trim();
        subscription.Status = request.Status;

        await _db.SaveChangesAsync(cancellationToken);
        return Ok(subscription.ToResponse());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var subscription = await _db.Subscriptions.FindAsync([id], cancellationToken);
        if (subscription is null) return NotFound();

        _db.Subscriptions.Remove(subscription);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:int}/debt")]
    public async Task<ActionResult<DebtQueryResponse>> QueryDebt(int id, CancellationToken cancellationToken)
    {
        var subscription = await _db.Subscriptions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (subscription is null) return NotFound();
        if (subscription.Status != SubscriptionStatus.Active) return BadRequest("Pasif abonelik için borç sorgulanamaz.");

        var debt = await _debtQueryService.QueryDebtAsync(subscription, cancellationToken);
        var isPaid = await _db.Payments.AnyAsync(p => p.SubscriptionId == id && p.Period == debt.Period && p.Status == PaymentStatus.Successful, cancellationToken);

        return Ok(debt with { IsPaidForPeriod = isPaid });
    }
}
