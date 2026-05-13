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
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IDebtQueryService _debtQueryService;
    private readonly IPaymentGatewayService _paymentGatewayService;

    public PaymentsController(AppDbContext db, IDebtQueryService debtQueryService, IPaymentGatewayService paymentGatewayService)
    {
        _db = db;
        _debtQueryService = debtQueryService;
        _paymentGatewayService = paymentGatewayService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentResponse>>> GetAll([FromQuery] int? customerId, [FromQuery] int? subscriptionId, CancellationToken cancellationToken)
    {
        var query = _db.Payments.Include(p => p.Subscription).AsNoTracking();
        if (subscriptionId.HasValue) query = query.Where(p => p.SubscriptionId == subscriptionId.Value);
        if (customerId.HasValue) query = query.Where(p => p.Subscription!.CustomerId == customerId.Value);

        var payments = await query
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => p.ToResponse())
            .ToListAsync(cancellationToken);

        return Ok(payments);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PaymentResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var payment = await _db.Payments.Include(p => p.Subscription).AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        return payment is null ? NotFound() : Ok(payment.ToResponse());
    }

    [HttpGet("subscription/{subscriptionId:int}/history")]
    public async Task<ActionResult<IEnumerable<PaymentResponse>>> GetSubscriptionHistory(int subscriptionId, CancellationToken cancellationToken)
    {
        var exists = await _db.Subscriptions.AnyAsync(s => s.Id == subscriptionId, cancellationToken);
        if (!exists) return NotFound();

        var history = await _db.Payments
            .Include(p => p.Subscription)
            .AsNoTracking()
            .Where(p => p.SubscriptionId == subscriptionId)
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => p.ToResponse())
            .ToListAsync(cancellationToken);

        return Ok(history);
    }

    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> Create(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var subscription = await _db.Subscriptions
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId, cancellationToken);

        if (subscription is null) return NotFound("Abonelik bulunamadı.");
        if (subscription.Status != SubscriptionStatus.Active) return BadRequest("Pasif abonelik için ödeme yapılamaz.");

        var debt = await _debtQueryService.QueryDebtAsync(subscription, cancellationToken);

        var alreadyPaid = await _db.Payments.AnyAsync(p =>
            p.SubscriptionId == subscription.Id &&
            p.Period == debt.Period &&
            p.Status == PaymentStatus.Successful,
            cancellationToken);

        if (alreadyPaid) return Conflict($"{debt.Period} dönemi için başarılı ödeme zaten kayıtlı.");

        var gatewayResponse = await _paymentGatewayService.PayAsync(
            new PaymentGatewayRequest(subscription.Id, debt.Amount, debt.Period),
            cancellationToken);

        var payment = new Payment
        {
            SubscriptionId = subscription.Id,
            Amount = debt.Amount,
            Period = debt.Period,
            PaymentDate = DateTime.UtcNow,
            Status = gatewayResponse.IsSuccessful ? PaymentStatus.Successful : PaymentStatus.Failed,
            TransactionReference = gatewayResponse.TransactionReference,
            FailureReason = gatewayResponse.ErrorMessage
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(cancellationToken);
        await _db.Entry(payment).Reference(p => p.Subscription).LoadAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment.ToResponse());
    }
}
