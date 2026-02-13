using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiscountManager.Modules.Customer.Infrastructure;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly CustomerDbContext _dbContext;

    public CustomerController(CustomerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetCustomer(string userId)
    {
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
        if (customer == null)
        {
            return NotFound();
        }
        return Ok(customer);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        var customer = new Domain.Customer(
            request.UserId,
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.Address,
            request.City,
            request.Country
        );

        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();
        return Ok(customer);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] UpdateCustomerRequest request)
    {
        var customer = await _dbContext.Customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        customer.UpdateProfile(request.FirstName, request.LastName, request.PhoneNumber, request.Address, request.City, request.Country);
        await _dbContext.SaveChangesAsync();
        return Ok(customer);
    }
}

public record CreateCustomerRequest(string UserId, string FirstName, string LastName, string? PhoneNumber, string? Address, string? City, string? Country);
public record UpdateCustomerRequest(string FirstName, string LastName, string? PhoneNumber, string? Address, string? City, string? Country);
