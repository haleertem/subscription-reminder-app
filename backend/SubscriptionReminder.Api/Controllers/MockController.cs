using Microsoft.AspNetCore.Mvc;
using SubscriptionReminder.Api.DTOs;
using SubscriptionReminder.Api.Models;

namespace SubscriptionReminder.Api.Controllers;

[ApiController]
[Route("api/mock")]
public class MockController : ControllerBase
{
    [HttpGet("debts")]
    public ActionResult<DebtQueryResponse> QueryDebt(
        [FromQuery] int subscriptionId,
        [FromQuery] SubscriptionType type,
        [FromQuery] string providerName,
        [FromQuery] string subscriptionNumber)
    {
        var today = DateTime.UtcNow.Date;
        var dueDate = new DateTime(today.Year, today.Month, Math.Min(28, Math.Max(1, today.Day + 3)));
        if (dueDate < today) dueDate = today.AddDays(3);

        var seed = Math.Abs(HashCode.Combine(providerName, subscriptionNumber, today.Year, today.Month));
        var amount = type switch
        {
            SubscriptionType.Electricity => 350 + seed % 500,
            SubscriptionType.Water => 120 + seed % 200,
            SubscriptionType.Internet => 250 + seed % 300,
            SubscriptionType.Gsm => 180 + seed % 250,
            SubscriptionType.NaturalGas => 300 + seed % 700,
            _ => 100 + seed % 400
        };

        return Ok(new DebtQueryResponse(
            subscriptionId,
            type,
            providerName,
            subscriptionNumber,
            amount,
            dueDate,
            today.ToString("yyyy-MM"),
            false));
    }

    [HttpPost("payments")]
    public ActionResult<PaymentGatewayResponse> Pay(PaymentGatewayRequest request)
    {
        var shouldFail = request.Amount % 13 == 0;
        if (shouldFail)
        {
            return Ok(new PaymentGatewayResponse(false, $"FAIL-{Guid.NewGuid():N}"[..18], "Mock banka ödeme provizyonu reddetti."));
        }

        return Ok(new PaymentGatewayResponse(true, $"PAY-{Guid.NewGuid():N}"[..18], null));
    }

    [HttpPost("notifications")]
    public ActionResult<NotificationResponse> Notify(NotificationRequest request)
    {
        return Ok(new NotificationResponse(true, request.Channel.ToString(), request.Recipient, request.Message));
    }
}
