using Microsoft.AspNetCore.Identity;

namespace DiscountManager.Modules.Identity.Domain;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
