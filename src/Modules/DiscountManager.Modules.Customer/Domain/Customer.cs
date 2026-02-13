using DiscountManager.Shared.SharedKernel.Domain;

namespace DiscountManager.Modules.Customer.Domain;

public class Customer : Entity, IAggregateRoot
{
    public string UserId { get; private set; } = default!; // Links to Identity.ApplicationUser.Id
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string? PhoneNumber { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? Country { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Customer() { } // EF Core

    public Customer(string userId, string firstName, string lastName, string? phoneNumber = null, string? address = null, string? city = null, string? country = null)
    {
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        Address = address;
        City = city;
        Country = country;
    }

    public void UpdateProfile(string firstName, string lastName, string? phoneNumber, string? address, string? city, string? country)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        Address = address;
        City = city;
        Country = country;
    }
}
