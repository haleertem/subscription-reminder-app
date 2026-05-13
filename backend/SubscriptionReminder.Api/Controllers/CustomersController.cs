using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubscriptionReminder.Api.Data;
using SubscriptionReminder.Api.DTOs;
using SubscriptionReminder.Api.Helpers;
using SubscriptionReminder.Api.Models;

namespace SubscriptionReminder.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _db;

    public CustomersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var customers = await _db.Customers
            .AsNoTracking()
            .OrderBy(c => c.FullName)
            .Select(c => c.ToResponse())
            .ToListAsync(cancellationToken);

        return Ok(customers);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var customer = await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        return customer is null ? NotFound() : Ok(customer.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> Create(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        if (await _db.Customers.AnyAsync(c => c.Email == request.Email, cancellationToken))
            return Conflict("Bu e-posta adresi ile kayıtlı müşteri var.");

        var customer = new Customer
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim()
        };

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer.ToResponse());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var customer = await _db.Customers.FindAsync([id], cancellationToken);
        if (customer is null) return NotFound();

        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
