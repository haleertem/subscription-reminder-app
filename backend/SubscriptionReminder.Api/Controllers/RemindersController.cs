using Microsoft.AspNetCore.Mvc;
using SubscriptionReminder.Api.DTOs;
using SubscriptionReminder.Api.Services;

namespace SubscriptionReminder.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RemindersController : ControllerBase
{
    private readonly IReminderService _reminderService;

    public RemindersController(IReminderService reminderService)
    {
        _reminderService = reminderService;
    }

    [HttpGet("customers/{customerId:int}/check")]
    public async Task<ActionResult<IEnumerable<ReminderResponse>>> Check(int customerId, [FromQuery] int days = 5, CancellationToken cancellationToken = default)
    {
        var result = await _reminderService.CheckCustomerRemindersAsync(customerId, days, cancellationToken);
        return Ok(result);
    }
}
